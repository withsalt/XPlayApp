using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.Data.Entity.User;

namespace App.Repository.User
{
    public class UserValidateLogRepository : BaseRepository<UserValidateLog>, IUserValidateLogRepository
    {
        private readonly ILogger<UserValidateLogRepository> _logger;

        public UserValidateLogRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<UserValidateLogRepository>();
        }
    }
}
