using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.IServices.User;
using App.Data.Entity.User;

namespace App.Services.User
{
    public class UserValidateLogService : BaseServices<UserValidateLog>, IUserValidateLogService
    {
        private readonly ILogger<UserValidateLogService> _logger;
        private readonly IUserValidateLogRepository _dal;

        public UserValidateLogService(ILoggerFactory logger,
            IUserValidateLogRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<UserValidateLogService>();
            this._dal = dal;
        }
    }
}
