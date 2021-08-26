using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.Data.Entity.System;

namespace App.Repository.System
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<RoleRepository>();
        }
    }
}
