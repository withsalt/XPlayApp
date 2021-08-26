using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.Data.Entity.System;
using System.Threading.Tasks;
using App.Data.Model.Request.Upload;
using System;
using App.Util.Date;

namespace App.Repository.System
{
    public class MaterialRepository : BaseRepository<Material>, IMaterialRepository
    {
        private readonly ILogger<MaterialRepository> _logger;

        public MaterialRepository(ILoggerFactory logger, IBaseDbContext context) : base(logger, context)
        {
            _logger = logger.CreateLogger<MaterialRepository>();
        }

        public async Task<bool> SaveFileInfo(FileUploadParam uploadParam)
        {
            try
            {
                Material entity = new Material()
                {
                    FileName = uploadParam.FileName,
                    FileOldName = uploadParam.FileOldName,
                    Path = uploadParam.Path,
                    LogoPath = uploadParam.Logo,
                    Duration = uploadParam.Duration,
                    Extension = uploadParam.Extension,
                    FileType = uploadParam.FileType,
                    Width = uploadParam.Width,
                    Height = uploadParam.Height,
                    Size = uploadParam.Size,
                    Remark = uploadParam.Remark,
                    CreateTime = TimeUtil.Timestamp()
                };
                int count = await db.Insertable(entity).ExecuteCommandAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save file to db failed. {ex.Message}", ex);
                return false;
            }
        }
    }
}
