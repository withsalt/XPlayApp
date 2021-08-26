using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;
using App.IRepository.User;
using App.Data.Entity.User;
using System.Threading.Tasks;
using System;
using App.Util.Date;
using SqlSugar;
using App.Data.Model.Request.Password;

namespace App.Repository.User
{
    public class UserPasswdRepository : BaseRepository<UserPasswd>, IUserPasswdRepository
    {
        private readonly ILogger<UserPasswdRepository> _logger;

        public UserPasswdRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<UserPasswdRepository>();
        }

        public async Task<bool> ResetPassword(ResetPasswordParams param)
        {
            try
            {
                UserPasswd passwd = await db.Queryable<UserPasswd>()
                    .Where(u => u.UserId == param.UserId)
                    .SingleAsync();
                if (passwd == null)
                {
                    return false;
                }

                passwd.Password = param.Password;
                passwd.UpdateTime = TimeUtil.Timestamp();

                int count = await db.Updateable(passwd)
                    .UpdateColumns(p => new
                    {
                        p.Password,
                        p.UpdateTime
                    })
                    .Where(p => p.UserId == passwd.UserId)
                    .ExecuteCommandAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save new password info failed. {ex.Message}", ex);
                return false;
            }
        }


        public async Task<bool> CheckOldPassword(string uid, string passwd)
        {
            try
            {
                UserInfo info = await db.Queryable<UserInfo, UserPasswd>((ui, up) => new object[]{
                    JoinType.Inner,ui.UserId == up.UserId,
                })
                    .Where((ui, up) => ui.UserId.Equals(uid)
                        && up.Password == passwd
                        && ui.IsActive
                        && !ui.IsDelete)
                    .SingleAsync();
                if(info != null)
                {
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Check old password failed. {ex.Message}", ex);
                return false;
            }
        }
    }
}
