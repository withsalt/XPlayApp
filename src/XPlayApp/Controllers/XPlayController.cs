using App.Config;
using App.Data.Entity.System;
using App.Data.Enum;
using App.Data.Model.Common;
using App.Data.Model.Common.JsonObject;
using App.IServices.System;
using CliWrap;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XPlayApp.Models.RquestModel;
using XPlayApp.Services.XPlay;
using XPlayApp.Services.XPlay.Enum;
using XPlayApp.Services.XPlay.Model;

namespace XPlayApp.Controllers
{
    public class XPlayController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<XPlayController> _logger;
        private readonly ConfigManager _config;
        private readonly IMaterialService _materialService;
        private readonly XPlayService _xplayService;

        public XPlayController(ILogger<XPlayController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMaterialService materialService
            , XPlayService xplayService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<XPlayController>));
            this._hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._xplayService = xplayService ?? throw new ArgumentNullException(nameof(XPlayService));
            this._materialService = materialService ?? throw new ArgumentNullException(nameof(IMaterialService));
        }

        [HttpPost]
        public async Task<IActionResult> Play(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new ResultModel<IChild>(10011, "播放文件ID不能为空。"));
                }
                Material material = await _materialService.QueryById(id);
                if (material == null)
                {
                    return Json(new ResultModel<IChild>(10012, "获取文件信息失败。"));
                }
                string filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.Path);
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new ResultModel<IChild>(10012, "播放的文件不存在。"));
                }
                material.Path = filePath;
                if (!_xplayService.ConnectStatus)
                {
                    return Json(new ResultModel<IChild>(10012, "播放服务离线。"));
                }
                (bool, string) result = (false, null);
                switch (material.FileType)
                {
                    case FileType.Video:
                        result = await _xplayService.PlayVideo(material);
                        break;
                    case FileType.Music:
                        result = await _xplayService.PlayAudio(material);
                        break;
                    case FileType.Image:
                        result = await _xplayService.PlayImage(material);
                        break;
                    default:
                        return Json(new ResultModel<IChild>(10013, "不支持的文件类型。"));
                }
                if (result.Item1)
                {
                    return Json(new ResultModel<IChild>(0));
                }
                else
                {
                    return Json(new ResultModel<IChild>(10013, result.Item2));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Json(new ResultModel<IChild>(10013, "系统错误，详细信息请查看日志。"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaySequence(PlaySequenceParams @params)
        {
            if (@params == null || @params.Items == null || @params.Items.Count == 0)
            {
                return Json(new ResultModel<IChild>(10021, "播放序列为空。"));
            }
            List<object> playIds = new List<object>();
            foreach (var item in @params.Items)
            {
                if (item.Id == 0)
                {
                    return Json(new ResultModel<IChild>(10021, "播放序列中包含无效的ID。"));
                }
                playIds.Add(item.Id);
            }
            List<Material> materials = await _materialService.QueryByIDs(playIds.ToArray());
            if (materials.Count != @params.Items.Count)
            {
                return Json(new ResultModel<IChild>(10022, "获取播放序列信息失败。"));
            }
            if (!_xplayService.ConnectStatus)
            {
                return Json(new ResultModel<IChild>(10012, "播放服务离线。"));
            }
            foreach (var item in materials)
            {
                string filePath = Path.Combine(_hostingEnvironment.WebRootPath, item.Path);
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new ResultModel<IChild>(10012, $"播放序列中包含不存在的文件，文件名：{item.FileOldName}。"));
                }
                item.Path = filePath;
            }
            (bool, string) result = await _xplayService.PlaySequence(materials);
            if (result.Item1)
            {
                return Json(new ResultModel<IChild>(0));
            }
            else
            {
                return Json(new ResultModel<IChild>(10013, result.Item2));
            }
        }

        [HttpPost]
        public IActionResult Reboot()
        {
            string command = "shutdown";
            string args = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                args = "-r -t 0 -f";
            }
            else
            {
                args = "-r now";
            }

            Task.Run(async () =>
            {
                await Task.Delay(500);
                CliWrap.CommandResult result = await Cli.Wrap(command)
                        .WithArguments(args)
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();
            });
            return Json(new ResultModel<IChild>(0));
        }

        [HttpPost]
        public IActionResult Shutdown()
        {
            string command = "shutdown";
            string args = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                args = "-s -t 0 -f";
            }
            else
            {
                args = "-h now";
            }

            Task.Run(async () =>
            {
                await Task.Delay(500);
                CliWrap.CommandResult result = await Cli.Wrap(command)
                        .WithArguments(args)
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();
            });
            return Json(new ResultModel<IChild>(0));
        }
    }
}
