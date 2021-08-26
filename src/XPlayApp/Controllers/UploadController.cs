using App.Config;
using App.Data.Entity.System;
using App.Data.Enum;
using App.Data.Model.Common;
using App.Data.Model.Common.JsonObject;
using App.Data.Model.Request.Upload;
using App.IServices.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.FFmpeg;
using OnceMi.AspNetCore.FFmpeg.Models;
using OnceMi.AspNetCore.FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using XPlayApp.Models.ResponseModel.FileUpload;

namespace XPlayApp.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<UploadController> _logger;
        private readonly ConfigManager _config;
        private readonly IMaterialService _materialService;
        private readonly FFmpegService _ffmpegService;

        public UploadController(ILogger<UploadController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMaterialService materialService
            , FFmpegService ffmpegService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<UploadController>));
            this._hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._materialService = materialService ?? throw new ArgumentNullException(nameof(IMaterialService));
            this._ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(FFmpegService));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> OnUpload(UploadParams uploadParams)
        {
            IFormFileCollection files = Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                return Json(new UploadResult()
                {
                    error = "上传文件为空。"
                });
            }
            foreach (var item in files)
            {
                uploadParams.uploadFiles.Add(item);
            }
            foreach (var item in uploadParams.uploadFiles)
            {
                string fileName = item.FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return Json(new UploadResult()
                    {
                        error = "上传文件文件名不能为空。"
                    });
                }
                string ext = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(ext))
                {
                    return Json(new UploadResult()
                    {
                        error = "后缀名不能为空。"
                    });
                }
                FileType fileType = FileType.Unknow;
                if (_config.AppSettings.AllowExtensions != null && _config.AppSettings.AllowExtensions.Count > 0)
                {
                    foreach (var fileTypeItem in _config.AppSettings.AllowExtensions)
                    {
                        if (fileTypeItem.Values != null && fileTypeItem.Values.Contains(ext.ToLower()))
                        {
                            fileType = fileTypeItem.Type;
                            break;
                        }
                    }
                }
                if (fileType == FileType.Unknow)
                {
                    return Json(new UploadResult()
                    {
                        error = "不支持的文件后后缀名。"
                    });
                }

                ext = ext.ToLower();
                string fileNewName = Guid.NewGuid().ToString() + ext;
                (string, string) fileSaveResult = await SaveFile(item, fileNewName);
                if (string.IsNullOrEmpty(fileSaveResult.Item1))
                {
                    return Json(new UploadResult()
                    {
                        error = "保存文件失败。"
                    });
                }

                string logoPath = null;
                int duration = 0;
                int width = 0;
                int height = 0;
                switch (fileType)
                {
                    case FileType.Video:
                        VideoInfo videoInfo = await _ffmpegService.VideoInfo(fileSaveResult.Item2);
                        if (videoInfo != null && videoInfo.Duration != 0)
                        {
                            width = videoInfo.Width;
                            height = videoInfo.Height;
                            duration = videoInfo.Duration;
                            //生成略缩图
                            logoPath = await VideoThumbnailGenerator(fileSaveResult.Item2, 640, 480, duration > 5 ? 5 : 1);
                            if (string.IsNullOrEmpty(logoPath))
                            {
                                logoPath = $"assets/img/logo/video.png";
                            }
                        }
                        else
                        {
                            logoPath = $"assets/img/logo/video.png";
                        }

                        break;
                    case FileType.Image:
                        (string, int, int) imageTn = ImageThumbnailGenerator(fileSaveResult.Item2);
                        if (string.IsNullOrEmpty(imageTn.Item1))
                        {
                            logoPath = $"assets/img/logo/image.png";
                        }
                        else
                        {
                            logoPath = imageTn.Item1;
                            width = imageTn.Item2;
                            height = imageTn.Item3;
                        }
                        break;
                    case FileType.Music:
                        VideoInfo audioInfo = await _ffmpegService.VideoInfo(fileSaveResult.Item2);
                        if (audioInfo != null)
                        {
                            duration = audioInfo.Duration;
                        }
                        else
                        {
                            duration = 0;
                        }
                        logoPath = $"assets/img/logo/music.png";
                        break;
                    default:
                        break;
                }

                FileUploadParam fileUploadModel = new FileUploadParam()
                {
                    FileName = fileNewName,
                    FileOldName = fileName,
                    Path = fileSaveResult.Item1,
                    Logo = logoPath,
                    Extension = ext,
                    Duration = duration,
                    FileType = fileType,
                    Width = width,
                    Height = height,
                    Size = item.Length / 1024
                };
                if (!await _materialService.SaveFileInfo(fileUploadModel))
                {
                    return Json(new UploadResult()
                    {
                        error = "写入文件信息到数据库失败。"
                    });
                }
            }

            //上传成功
            return Json(new UploadResult());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("文件ID不能为空。");
                }
                Material item = await _materialService.QueryById(id);
                if (item == null)
                {
                    throw new Exception("查找文件信息失败。");
                }
                bool result = await _materialService.DeleteById(id);
                if (!result)
                {
                    throw new Exception("删除文件失败。");
                }
                string filePath = Path.Combine(_hostingEnvironment.WebRootPath, item.Path);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                if (!string.IsNullOrEmpty(item.LogoPath) && item.LogoPath.StartsWith("data/"))
                {
                    string logoPath = Path.Combine(_hostingEnvironment.WebRootPath, item.LogoPath);
                    if (System.IO.File.Exists(logoPath))
                    {
                        System.IO.File.Delete(logoPath);
                    }
                }
                return Json(new ResultModel<IChild>(0));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete file by id({id}) failed. {ex.Message}", ex);
                return Json(new ResultModel<IChild>(1001, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> FileInfo(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new ResultModel<IChild>(10031, "文件ID不能为空。"));
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

                return Json(new ResultModel<FileInfoResult>(0)
                {
                    Data = new FileInfoResult()
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete file by id({id}) failed. {ex.Message}", ex);
                return Json(new ResultModel<IChild>(10031, ex.Message));
            }
        }

        private async Task<(string, string)> SaveFile(IFormFile formFile, string fileNewName)
        {
            try
            {
                if (formFile == null)
                {
                    return (null, null);
                }
                if (string.IsNullOrEmpty(fileNewName))
                {
                    return (null, null);
                }
                string dataPath = Path.Combine("data", DateTime.Now.ToString("yyyyMMdd"));
                string savePath = Path.Combine(_hostingEnvironment.WebRootPath, dataPath);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string fileName = Path.Combine(savePath, fileNewName);
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    var inputStream = formFile.OpenReadStream();
                    await inputStream.CopyToAsync(fileStream, 80 * 1024, default);
                }
                if (System.IO.File.Exists(fileName))
                {
                    return (Path.Combine(dataPath, fileNewName), fileName);
                }
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save upload file failed. {ex.Message}", ex);
                return (null, null);
            }
        }

        /// <summary>
        /// 生成图片略缩图
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private (string, int, int) ImageThumbnailGenerator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return (null, 0, 0);
            }
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    throw new Exception("Process img is not exist.");
                }
                string imgDestPath = Path.Combine("data", DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid().ToString() + ".jpg");
                string imgDestFullPath = Path.Combine(_hostingEnvironment.WebRootPath, imgDestPath);
                if (System.IO.File.Exists(imgDestFullPath))
                {
                    System.IO.File.Delete(imgDestFullPath);
                }
                int width = 0;
                int height = 0;
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (Image img = Image.FromStream(fs))
                    {
                        width = img.Width;
                        height = img.Height;
                        using (Image scaled = ImageTools.ResizeImage(img, 640, 480))
                        {
                            using (Bitmap bitmap = new Bitmap(scaled))
                            {
                                bitmap.Save(imgDestFullPath, ImageFormat.Jpeg);
                            }
                        }
                    }
                }
                if (System.IO.File.Exists(imgDestFullPath))
                {
                    return (imgDestPath.Replace("\\", "/").TrimStart('/'), width, height);
                }
                else
                {
                    return (null, width, height);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Build thumbNail image failed, {ex.Message}", ex);
                return (null, 0, 0);
            }
        }

        /// <summary>
        /// 从视频画面中截取一帧画面为图片
        /// </summary>
        /// <param name="videoName">视频文件路径pic/123.MP4</param>
        /// <param name="widthAndHeight">图片的尺寸如:240*180</param>
        /// <param name="cutTimeFrame">开始截取的时间如:"1s"</param>
        /// <returns>返回图片保存路径</returns>
        private async Task<string> VideoThumbnailGenerator(string videoPath, int width, int height, int cutTime = 5)
        {
            try
            {
                if (string.IsNullOrEmpty(videoPath))
                {
                    return null;
                }
                string imgDestPath = Path.Combine("data", DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid().ToString() + ".jpg");
                string imgDestFullPath = Path.Combine(_hostingEnvironment.WebRootPath, imgDestPath);
                if (System.IO.File.Exists(imgDestFullPath))
                {
                    System.IO.File.Delete(imgDestFullPath);
                }
                using (MemoryStream stream = await _ffmpegService.VideoThumbnailGenerator(videoPath, width, height, cutTime))
                {
                    if (stream == null || stream.Length == 0)
                    {
                        return null;
                    }
                    stream.Position = 0;
                    using (Image img = Image.FromStream(stream, false))
                    {
                        using (Bitmap bitmap = new Bitmap(img))
                        {
                            bitmap.Save(imgDestFullPath, ImageFormat.Jpeg);
                        }
                    }

                    if (System.IO.File.Exists(imgDestFullPath))
                    {
                        return imgDestPath.Replace("\\", "/").TrimStart('/');
                    }
                    return null;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create video logo failed. {ex.Message}", ex);
                return null;
            }
        }


    }
}
