using App.Config;
using App.Data.Entity.System;
using App.Data.Enum;
using App.Data.Model.Common;
using App.Data.Model.Common.JsonObject;
using App.IServices.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XPlayApp.Models;
using XPlayApp.Models.ResponseModel.FileUpload;
using XPlayApp.Models.ViewModels.Videos;

namespace XPlayApp.Controllers
{
    public class VideoController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<VideoController> _logger;
        private readonly ConfigManager _config;
        private readonly IMaterialService _materialService;

        public VideoController(ILogger<VideoController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMaterialService materialService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<VideoController>));
            this._hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._materialService = materialService ?? throw new ArgumentNullException(nameof(IMaterialService));
        }

        public async Task<IActionResult> Index()
        {
            VideoPageViewModel vm = new VideoPageViewModel();
            List<Material> materials = await _materialService.Query(m => m.FileType == FileType.Video);
            if (materials == null || materials.Count == 0)
            {
                return View(vm);
            }
            string hostUrl = Request.Host.Value;

            string baseHost = $"{Request.Scheme}://{Request.Host}/";
            foreach (var item in materials)
            {
                item.LogoPath = baseHost + item.LogoPath;
            }
            vm.Materials = materials;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Play(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Content("文件ID不能为空。");
            }
            Material item = await _materialService.QueryById(id);
            if (item == null)
            {
                return Json(new ResultModel<IChild>(10033, "查找文件信息失败。"));
            }
            string baseHost = $"{Request.Scheme}://{Request.Host}/";
            item.LogoPath = baseHost + item.LogoPath;

            string contentType = "application/octet-stream";
            string fileName = Path.GetFileName(item.FileOldName);
            if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }

            VideoPlayViewModel vm = new VideoPlayViewModel()
            {
                VideoFileInfo = new FileInfoResult()
                {
                    Id = item.Id,
                    FileName = item.FileName,
                    FileOldName = item.FileOldName,
                    Path = item.Path,
                    LogoPath = item.LogoPath,
                    Duration = item.Duration,
                    Extension = item.Extension,
                    FileType = item.FileType,
                    Size = item.Size,
                    Remark = item.Remark,
                    CreateTime = item.CreateTime,
                    ContentType = contentType
                }
            };
            return View(vm);
        }
    }
}
