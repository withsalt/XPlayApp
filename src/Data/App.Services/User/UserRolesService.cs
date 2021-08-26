using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.IServices.User;
using App.Data.Entity.User;

namespace App.Services.User
{
    public class UserRolesService : BaseServices<UserRoles>, IUserRolesService
    {
        private readonly ILogger<UserRolesService> _logger;
        private readonly IUserRolesRepository _dal;

        public UserRolesService(ILoggerFactory logger,
            IUserRolesRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<UserRolesService>();
            this._dal = dal;
        }
    }
}
