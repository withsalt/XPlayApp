using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.Data.Entity.System;

namespace App.Repository.System
{
    public class SystemLogRepository : BaseRepository<SystemLog>, ISystemLogRepository
    {
        private readonly ILogger<SystemLogRepository> _logger;

        public SystemLogRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<SystemLogRepository>();
        }
    }
}
