using App.Config;
using App.Data.Entity.System;
using App.Data.Enum;
using App.Data.Model.Common;
using App.Data.Model.Common.JsonObject;
using App.IServices.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XPlayApp.Models;
using XPlayApp.Models.ViewModels.Videos;

namespace XPlayApp.Controllers
{
    public class AudioController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<AudioController> _logger;
        private readonly ConfigManager _config;
        private readonly IMaterialService _materialService;

        public AudioController(ILogger<AudioController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMaterialService materialService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<AudioController>));
            this._hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._materialService = materialService ?? throw new ArgumentNullException(nameof(IMaterialService));
        }

        public async Task<IActionResult> Index()
        {
            VideoPageViewModel vm = new VideoPageViewModel();
            List<Material> materials = await _materialService.Query(m => m.FileType == FileType.Music);
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
    }
}
