using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.Data.Entity.System;

namespace App.Repository.System
{
    public class MenuPermissionRepository : BaseRepository<MenuPermission>, IMenuPermissionRepository
    {
        private readonly ILogger<MenuPermissionRepository> _logger;

        public MenuPermissionRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<MenuPermissionRepository>();
        }
    }
}
