using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.IServices.System;
using App.Data.Entity.System;

namespace App.Services.System
{
    public class RoleService : BaseServices<Role>, IRoleService
    {
        private readonly ILogger<RoleService> _logger;
        private readonly IRoleRepository _dal;

        public RoleService(ILoggerFactory logger,
            IRoleRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<RoleService>();
            this._dal = dal;
        }
    }
}
