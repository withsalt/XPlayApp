using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.Data.Entity.User;
using System;
using System.Threading.Tasks;
using App.Util.Date;
using App.Data.Model.Response.User;
using App.Util.User;
using System.Collections.Generic;
using SqlSugar;
using App.Config;
using System.Linq;
using App.Util.Security;
using App.Repository.Util;
using Microsoft.Extensions.Caching.Memory;

namespace App.Repository.User
{
    public class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
        private readonly ILogger<UserInfoRepository> _logger;
        private readonly ConfigManager _config;
        private readonly IMemoryCache _cache;

        public UserInfoRepository(ILoggerFactory logger
            , IBaseDbContext context
            , ConfigManager config
            , IMemoryCache redis) : base(logger, context)
        {
            _logger = logger.CreateLogger<UserInfoRepository>();
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _cache = redis ?? throw new ArgumentNullException(nameof(IMemoryCache));
        }

        #region Get Userinfo

        /// <summary>
        /// 用户账号和密码登录
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="pssswd"></param>
        /// <returns></returns>
        public async Task<UserInfo> Login(string loginInfo, string passwd)
        {
            if (string.IsNullOrEmpty(loginInfo))
            {
                return null;
            }
            try
            {
                UserInfo info = await db.Queryable<UserInfo, UserPasswd>((ui, up) => new object[]{
                    JoinType.Inner,ui.UserId == up.UserId,
                })
                    .Where((ui, up) => (ui.UserId.Equals(loginInfo)
                        || ui.Phone.Equals(loginInfo)
                        || ui.Email.Equals(loginInfo))
                        && up.Password == passwd
                        && ui.IsActive
                        && !ui.IsDelete)
                    .Mapper((ui, cache) =>
                    {
                        List<UserPasswd> ups = cache.GetListByPrimaryKeys<UserPasswd>(up => up.UserId);
                        if (ups != null && ups.Count > 0)
                        {
                            ui.UserPasswd = ups.Where(u => u.UserId == ui.UserId).SingleOrDefault();
                        }
                    })
                    .SingleAsync();
                if (info == null || !await UpdateLoginInfo(info, info.UserId))
                {
                    return null;
                }
                else
                {
                    return info;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get user info failed, login info is {loginInfo}. {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 用户账号和密码登录
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="pssswd"></param>
        /// <returns></returns>
        public async Task<UserInfo> LoginByStateCookie(string uid, string Md5Passwd, string token)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(Md5Passwd) || string.IsNullOrEmpty(token))
            {
                return null;
            }
            try
            {
                //验证token
                UserTokenInfo tokenInfo = await GetUserToken(uid, token);
                if (tokenInfo == null || tokenInfo.UserId != uid)
                {
                    return null;
                }
                //验证用户
                UserInfo info = await db.Queryable<UserInfo, UserPasswd>((ui, up) => new object[]{
                    JoinType.Inner,ui.UserId == up.UserId,
                })
                    .Where((ui, up) => ui.UserId.Equals(uid) && ui.IsActive && !ui.IsDelete)
                    .Mapper((ui, cache) =>
                    {
                        List<UserPasswd> ups = cache.GetListByPrimaryKeys<UserPasswd>(up => up.UserId);
                        if (ups != null && ups.Count > 0)
                        {
                            ui.UserPasswd = ups.Where(u => u.UserId == ui.UserId).SingleOrDefault();
                        }
                    })
                    .SingleAsync();

                if (info != null && Md5Passwd.Equals(Encrypt.MDString(info.UserPasswd.Password), StringComparison.OrdinalIgnoreCase))
                {
                    return info;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get user info failed, login uid is {uid}, error : {ex.Message}", ex);
                return null;
            }
        }

        public async Task<UserInfo> LoginByUserCookie(string uid, string Md5Passwd)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(Md5Passwd))
            {
                return null;
            }
            try
            {
                UserInfo info = await db.Queryable<UserInfo, UserPasswd>((ui, up) => new object[]{
                    JoinType.Inner,ui.UserId == up.UserId,
                })
                    .Where((ui, up) => ui.UserId.Equals(uid) && ui.IsActive && !ui.IsDelete)
                    .Mapper((ui, cache) =>
                    {
                        List<UserPasswd> ups = cache.GetListByPrimaryKeys<UserPasswd>(up => up.UserId);
                        if (ups != null && ups.Count > 0)
                        {
                            ui.UserPasswd = ups.Where(u => u.UserId == ui.UserId).SingleOrDefault();
                        }
                    })
                    .SingleAsync();

                if (info != null
                    && Md5Passwd.Equals(Encrypt.MDString(info.UserPasswd.Password), StringComparison.OrdinalIgnoreCase)
                    && await UpdateLoginInfo(info, "cookie"))
                {
                    return info;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get user info failed, login uid is {uid}, error : {ex.Message}", ex);
                return null;
            }
        }
        #endregion

        #region Token

        public async Task<bool> SetUserToken(string uid, string token)
        {
            try
            {
                UserTokenInfo tokenInfo = new UserTokenInfo()
                {
                    UserId = uid,
                    Token = token,
                    Time = TimeUtil.Timestamp()
                };
                return await SetUserToken(tokenInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save user token to redis failed. error msg is {ex.Message}", ex);
                return false;
            }
        }

        public Task<bool> SetUserToken(UserTokenInfo tokenInfo)
        {
            try
            {
                _cache.Set($"USER_TOKEN_{tokenInfo.UserId}", tokenInfo, TimeSpan.FromDays(7));
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save user token to redis failed. error msg is {ex.Message}", ex);
                return Task.FromResult(false);
            }
        }

        public async Task<UserTokenInfo> GetUserToken(string uid, string token)
        {
            try
            {
                UserTokenInfo redisData = _cache.Get<UserTokenInfo>($"USER_TOKEN_{uid}");
                if (redisData == null)
                {
                    if (await TokenValidate(uid, token))
                    {
                        UserTokenInfo tokenInfo = new UserTokenInfo()
                        {
                            UserId = uid,
                            Token = token,
                            Time = TimeUtil.Timestamp()
                        };
                        if (!await SetUserToken(tokenInfo))
                        {
                            return null;
                        }
                        return tokenInfo;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return redisData;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get user token to redis failed. error msg is {ex.Message}", ex);
                return null;
            }
        }


        #endregion

        #region 辅助

        /// <summary>
        /// 保存登录信息并更新Token
        /// </summary>
        /// <param name="info"></param>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        private async Task<bool> UpdateLoginInfo(UserInfo info, string loginInfo)
        {
            if (info == null || string.IsNullOrEmpty(loginInfo))
            {
                return false;
            }
            try
            {
                UserPasswd passwd = await db.Queryable<UserPasswd>()
                    .Where(u => u.UserId == info.UserId)
                    .SingleAsync();
                if (passwd == null)
                {
                    return false;
                }
                //token管理
                string token = UserUtil.GenerateToken(info.UserId);
                //Set token
                if (!await SetUserToken(info.UserId, token))
                {
                    return false;
                }

                passwd.Token = token;
                passwd.UpdateTime = TimeUtil.Timestamp();
                info.UserPasswd.Token = token;

                LoginHistory history = new LoginHistory()
                {
                    Userid = info.UserId,
                    TypeId = 1,
                    LoginId = loginInfo,
                    LoginTime = TimeUtil.Timestamp(),
                };

                db.Ado.BeginTran();
                int count = await db.Updateable(passwd)
                    .UpdateColumns(col => new { col.Token, col.UpdateTime })
                    .Where(pd => pd.UserId == passwd.UserId)
                    .ExecuteCommandAsync();
                if (count <= 0)
                    throw new Exception("Update user token info failed.");
                count = await db.Insertable(history).ExecuteCommandAsync();
                if (count <= 0)
                    throw new Exception("Save user login history info failed.");
                db.Ado.CommitTran();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Set login info failed, login info is {loginInfo}, {ex.Message}", ex);
                db.Ado.RollbackTran();
                return false;
            }
        }

        private async Task<bool> TokenValidate(string uid, string token)
        {
            try
            {
                int count = await db.Queryable<UserPasswd>()
                    .Where(u => u.Token == token && u.UserId == uid)
                    .CountAsync();
                return count > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
