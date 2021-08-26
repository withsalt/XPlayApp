using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.Data.Entity.User;

namespace App.Repository.User
{
    public class UserRolesRepository : BaseRepository<UserRoles>, IUserRolesRepository
    {
        private readonly ILogger<UserRolesRepository> _logger;

        public UserRolesRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<UserRolesRepository>();
        }
    }
}
