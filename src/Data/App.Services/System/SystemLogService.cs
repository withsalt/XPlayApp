using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.IServices.System;
using App.Data.Entity.System;

namespace App.Services.System
{
    public class SystemLogService : BaseServices<SystemLog>, ISystemLogService
    {
        private readonly ILogger<SystemLogService> _logger;
        private readonly ISystemLogRepository _dal;

        public SystemLogService(ILoggerFactory logger,
            ISystemLogRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<SystemLogService>();
            this._dal = dal;
        }
    }
}
