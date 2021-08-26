using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.IServices.User;
using App.Data.Entity.User;
using App.Data.Model.Response.User;
using System.Threading.Tasks;
using App.Util.User;
using System;
using AutoMapper;
using App.Config;

namespace App.Services.User
{
    public class UserInfoService : BaseServices<UserInfo>, IUserInfoService
    {
        private readonly ILogger<UserInfoService> _logger;
        private readonly IUserInfoRepository _dal;
        private readonly ConfigManager _config;
        private readonly IMapper _mapper;

        public UserInfoService(ILoggerFactory logger
            , IUserInfoRepository dal
            , ConfigManager config
            , IMapper mapper) : base(dal)
        {
            this._logger = logger.CreateLogger<UserInfoService>();
            this._dal = dal;
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(IMapper));
        }

        #region 用户登录

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="passwd"></param>
        /// <param name="errorid"></param>
        /// <returns></returns>
        public async Task<(int, UserInfoModel)> Login(string loginInfo, string passwd)
        {
            if (string.IsNullOrEmpty(loginInfo))
            {
                return (30151, null);
            }
            if (string.IsNullOrEmpty(passwd))
            {
                return (30152, null);
            }
            //密码加盐
            passwd = UserUtil.PasswdAddSalt(passwd, _config.AppSettings.PasswdSalt);
            //登录验证
            UserInfo user = await _dal.Login(loginInfo, passwd);
            if (user == null)
            {
                return (30154, null);
            }
            else
            {
                return (0, _mapper.Map<UserInfoModel>(user));
            }
        }

        public async Task<UserInfoModel> LoginByStateCookie(string uid, string Md5Passwd, string token)
        {
            UserInfo user = await _dal.LoginByStateCookie(uid, Md5Passwd, token);
            return _mapper.Map<UserInfoModel>(user);
        }

        public async Task<UserInfoModel> LoginByUserCookie(string uid, string Md5Passwd)
        {
            UserInfo user = await _dal.LoginByUserCookie(uid, Md5Passwd);
            return _mapper.Map<UserInfoModel>(user);
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> ValidateToken(string uid, string token)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token))
            {
                return false;
            }
            UserTokenInfo tokenInfo = await _dal.GetUserToken(uid, token);
            if (tokenInfo != null && (tokenInfo.Token == token && tokenInfo.UserId == uid))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
