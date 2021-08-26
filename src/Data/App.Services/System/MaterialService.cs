using App.Services.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.IServices.System;
using App.Data.Entity.System;
using System.Threading.Tasks;
using App.Data.Model.Request.Upload;

namespace App.Services.System
{
    public class MaterialService : BaseServices<Material>, IMaterialService
    {
        private readonly ILogger<MaterialService> _logger;
        private readonly IMaterialRepository _dal;

        public MaterialService(ILoggerFactory logger,
            IMaterialRepository dal) : base(dal)
        {
            this._logger = logger.CreateLogger<MaterialService>();
            this._dal = dal;
        }

        public async Task<bool> SaveFileInfo(FileUploadParam uploadParam)
        {
            return await _dal.SaveFileInfo(uploadParam);
        }
    }
}
