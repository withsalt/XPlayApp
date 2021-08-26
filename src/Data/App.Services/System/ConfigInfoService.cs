using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.IServices.System;
using App.Data.Entity.System;

namespace App.Services.System
{
    public class ConfigInfoService : BaseServices<ConfigInfo>, IConfigInfoService
    {
        private readonly ILogger<ConfigInfoService> _logger;
        private readonly IConfigInfoRepository _dal;

        public ConfigInfoService(ILoggerFactory logger,
            IConfigInfoRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<ConfigInfoService>();
            this._dal = dal;
        }
    }
}
