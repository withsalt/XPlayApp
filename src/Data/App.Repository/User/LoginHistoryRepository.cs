using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.User;
using App.Data.Entity.User;

namespace App.Repository.User
{
    public class LoginHistoryRepository : BaseRepository<LoginHistory>, ILoginHistoryRepository
    {
        private readonly ILogger<LoginHistoryRepository> _logger;

        public LoginHistoryRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<LoginHistoryRepository>();
        }
    }
}
