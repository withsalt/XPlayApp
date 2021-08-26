using App.Config;
using App.Data.Entity.System;
using App.IServices.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XPlayApp.Controllers
{
    public class DownloadController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<DownloadController> _logger;
        private readonly ConfigManager _config;
        private readonly IMaterialService _materialService;

        public DownloadController(ILogger<DownloadController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMaterialService materialService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<DownloadController>));
            this._hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._materialService = materialService ?? throw new ArgumentNullException(nameof(IMaterialService));
        }

        public async Task<IActionResult> Index(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "文件ID不能为空。"
                    };
                }
                Material material = await _materialService.QueryById(id);
                if (material == null)
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "获取文件信息失败。"
                    };
                }
                string path = Path.Combine(_hostingEnvironment.WebRootPath, material.Path);
                if (!System.IO.File.Exists(path))
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "当前文件不存在。"
                    };
                }
                string contentType = "application/octet-stream";
                string fileName = Path.GetFileName(path);
                if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType))
                {
                    contentType = "application/octet-stream";
                }

                FileStream fs = new FileStream(path, FileMode.Open);
                return File(fs, contentType, material.FileOldName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Download file failed. {ex.Message}", ex);
                return new ContentResult()
                {
                    StatusCode = 404,
                    Content = $"获取文件失败，错误：{ex.Message}"
                };
            }
        }
    }
}
