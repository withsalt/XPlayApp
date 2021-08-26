using App.Config;
using App.IRepository.Interface;
using App.Repository.Interface;
using App.Repository.System;
using App.Repository.User;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;

namespace App.Repository
{
    public class CustumDbContext : BaseDbContext
    {
        private readonly ILoggerFactory _logger;
        private readonly ConfigManager _config;
        private readonly IMemoryCache _cache;

        public CustumDbContext(ILoggerFactory logger
            , ConfigManager config
            , IMemoryCache cache) : base(logger, config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(CustumDbContext));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _cache = cache ?? throw new ArgumentNullException(nameof(IMemoryCache));
        }

        #region User
        public LoginHistoryRepository LoginHistory { get { return new LoginHistoryRepository(_logger, this); } }
        public UserInfoRepository UserInfo { get { return new UserInfoRepository(_logger, this, _config, _cache); } }
        public UserPasswdRepository UserPasswd { get { return new UserPasswdRepository(_logger, this); } }
        public UserRolesRepository UserRoles { get { return new UserRolesRepository(_logger, this); } }
        public UserValidateLogRepository UserValidateLog { get { return new UserValidateLogRepository(_logger, this); } }
        #endregion

        #region System

        public ConfigInfoRepository ConfigInfo { get { return new ConfigInfoRepository(_logger, this, _cache); } }

        public MenuPermissionRepository MenuPermission { get { return new MenuPermissionRepository(_logger, this); } }

        public RoleRepository Role { get { return new RoleRepository(_logger, this); } }

        public SystemLogRepository SystemLog { get { return new SystemLogRepository(_logger, this); } }

        public MaterialRepository Material { get { return new MaterialRepository(_logger, this); } }
        #endregion
    }
}
