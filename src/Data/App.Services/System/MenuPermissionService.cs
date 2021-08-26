using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.IServices.System;
using App.Data.Entity.System;

namespace App.Services.System
{
    public class MenuPermissionService : BaseServices<MenuPermission>, IMenuPermissionService
    {
        private readonly ILogger<MenuPermissionService> _logger;
        private readonly IMenuPermissionRepository _dal;

        public MenuPermissionService(ILoggerFactory logger,
            IMenuPermissionRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<MenuPermissionService>();
            this._dal = dal;
        }
    }
}
