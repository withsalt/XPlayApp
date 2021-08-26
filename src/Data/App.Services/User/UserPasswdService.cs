using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.IServices.User;
using App.Data.Entity.User;
using System.Threading.Tasks;
using App.Data.Model.Request.Password;
using System;
using App.Util.User;
using App.Config;

namespace App.Services.User
{
    public class UserPasswdService : BaseServices<UserPasswd>, IUserPasswdService
    {
        private readonly ILogger<UserPasswdService> _logger;
        private readonly IUserPasswdRepository _dal;
        private readonly ConfigManager _config;

        public UserPasswdService(ILoggerFactory logger
            , IUserPasswdRepository dal
            , ConfigManager config) : base(dal)
        {
            this._logger = logger.CreateLogger<UserPasswdService>();
            this._dal = dal;
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
        }

        public async Task<(bool, int)> ResetPassword(ResetPasswordParams param)
        {
            if (string.IsNullOrEmpty(param.UserId))
            {
                return (false, 10206);
            }
            //æ…√‹¬Îº”—Œ
            param.OldPassword = UserUtil.PasswdAddSalt(param.OldPassword, _config.AppSettings.PasswdSalt);
            //æ…√‹¬Î—È÷§
            if (!await _dal.CheckOldPassword(param.UserId, param.OldPassword))
            {
                return (false, 10207);
            }
            //–¬√‹¬Îº”—Œ
            param.Password = UserUtil.PasswdAddSalt(param.Password, _config.AppSettings.PasswdSalt);
            if (await _dal.ResetPassword(param))
            {
                return (true, 0);
            }
            else
            {
                return (false, 10208);
            }
        }
    }
}
