using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.IServices.User;
using App.Data.Entity.User;

namespace App.Services.User
{
    public class LoginHistoryService : BaseServices<LoginHistory>, ILoginHistoryService
    {
        private readonly ILogger<LoginHistoryService> _logger;
        private readonly ILoginHistoryRepository _dal;

        public LoginHistoryService(ILoggerFactory logger,
            ILoginHistoryRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<LoginHistoryService>();
            this._dal = dal;
        }
    }
}
