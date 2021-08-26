using CliWrap;
using OnceMi.AspNetCore.FFmpeg.Models;
using OnceMi.AspNetCore.FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.FFmpeg
{
    public class FFmpegService
    {
        private string _ffmpegPath
        {
            get
            {
                //系统类型
                string osType;
                string ffmpegName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    osType = "windows";
                    ffmpegName = "ffmpeg.exe";
                }
                else
                {
                    osType = "linux";
                    ffmpegName = "ffmpeg";
                }
                //架构
                string platform;
                Architecture architecture = RuntimeInformation.OSArchitecture;
                if (architecture == Architecture.Arm || architecture == Architecture.Arm64)
                {
                    platform = "arm";
                }
                else if (architecture == Architecture.X64 || architecture == Architecture.X86)
                {
                    platform = "x64";
                }
                else
                {
                    throw new Exception($"Unsupport architecture type({architecture})");
                }

                string ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg", osType, platform, ffmpegName);
                return ffmpegPath;
            }
        }

        public FFmpegService()
        {

        }

        #region Init

        internal bool Init()
        {
            try
            {
                if (!File.Exists(_ffmpegPath))
                {
                    throw new Exception("ffmpef not exist.");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var startInfo = new ProcessStartInfo("chmod");
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.Arguments = $" +x \"{_ffmpegPath}\"";

                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Video

        public async Task<MemoryStream> VideoThumbnailGenerator(string videoPath, int width = 640, int height = 480, int cutTime = 5)
        {
            if (string.IsNullOrEmpty(videoPath))
            {
                return null;
            }
            if (cutTime <= 0)
            {
                return null;
            }
            StringBuilder errorOutputSb = new StringBuilder();
            using (FileStream inputStream = new FileStream(videoPath, FileMode.Open))
            {
                using (MemoryStream destStream = new MemoryStream())
                {
                    CommandResult result = await Cli.Wrap(_ffmpegPath)
                        .WithStandardOutputPipe(PipeTarget.ToStream(destStream))
                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorOutputSb))
                        .WithArguments($"-v quiet -ss {cutTime} -i {videoPath} -an -frames:v 1 -f image2pipe pipe:.jpg")
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();
                    if (result.ExitCode != 0)
                    {
                        throw new Exception(errorOutputSb.ToString());
                    }
                    if (destStream.Length == 0)
                    {
                        throw new Exception("ffmpeg result data is null.");
                    }
                    destStream.Position = 0;
                    if (!destStream.TryGetBuffer(out ArraySegment<byte> buffer) || buffer.Array == null)
                    {
                        throw new Exception("Get ffmpeg result memory stream byte buffer failed.");
                    }
                    ImageConverter imageConverter = new ImageConverter();
                    using (Image img = imageConverter.ConvertFrom(buffer.Array) as Image)
                    {
                        using (var scaled = ImageTools.ResizeImage(img, width, height))
                        {
                            var rv = new MemoryStream();
                            try
                            {
                                scaled.Save(rv, ImageFormat.Jpeg);
                                rv.Position = 0;
                                return rv;
                            }
                            catch (Exception)
                            {
                                rv.Dispose();
                                throw;
                            }
                        }
                    }
                }
            }
        }

        public async Task<VideoInfo> VideoInfo(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath))
            {
                return null;
            }

            StringBuilder outputSb = new StringBuilder();

            await Cli.Wrap(_ffmpegPath)
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(outputSb))
                .WithArguments($" -i \"{videoPath}\"")
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            if (outputSb.Length == 0)
            {
                throw new Exception("Get duration info failed.");
            }
            List<string> fileLineInfo = new List<string>(outputSb.ToString().Split("\n"));
            if (fileLineInfo.Count == 0)
            {
                throw new Exception("Get duration info failed.");
            }
            VideoInfo info = new VideoInfo();
            bool hasDuration = false;
            bool hasDetail = false;
            foreach (var item in fileLineInfo)
            {
                if (item.Contains("Duration:", StringComparison.OrdinalIgnoreCase) && !hasDuration)
                {
                    info.Duration = GetVideoDuration(item);
                    hasDuration = true;
                }
                if(item.Contains("Stream #0:", StringComparison.OrdinalIgnoreCase)
                    && item.Contains("Video:", StringComparison.OrdinalIgnoreCase)
                    && !hasDetail)
                {
                    VideoInfo detail = GetVideoDetail(item);
                    if(detail != null)
                    {
                        info.Encode = detail.Encode;
                        info.Format = detail.Format;
                        info.Width = detail.Width;
                        info.Height = detail.Height;
                        info.FPS = detail.FPS;
                        hasDetail = true;
                    }
                }
                if(hasDuration && hasDetail)
                {
                    break;
                }
            }
            return info;
        }

        private int GetVideoDuration(string input)
        {
            try
            {
                string[] allKeys = input.Split(',');
                if (allKeys.Length == 0)
                {
                    return 0;
                }
                string itemKey = null;
                foreach (var suraKey in allKeys)
                {
                    if (suraKey.Contains("Duration:", StringComparison.OrdinalIgnoreCase))
                    {
                        itemKey = suraKey.Trim('\r', '\n', ' ');
                        if (!string.IsNullOrEmpty(itemKey))
                        {
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(itemKey))
                {
                    return 0;
                }

                int firstIndex = itemKey.IndexOf(':');
                string durationVal = itemKey.Substring(firstIndex + 1);
                if (string.IsNullOrEmpty(durationVal))
                {
                    return 0;
                }
                durationVal = durationVal.Replace(" ", "");
                string[] durationVals = durationVal.Split(":");
                int ses = 0;
                for (int i = durationVals.Length - 1; i >= 0; i--)
                {
                    if (!float.TryParse(durationVals[i], out float val))
                    {
                        continue;
                    }
                    if (val == 0)
                    {
                        continue;
                    }
                    int loc = durationVals.Length - 1 - i;
                    if (loc == 0)
                    {
                        ses += (int)val;
                    }
                    else
                    {
                        ses += (int)val * 60 * loc;
                    }
                }
                return ses;
            }
            catch
            {
                return 0;
            }
        }

        private VideoInfo GetVideoDetail(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return null;
                }
                int startIndex = input.IndexOf("Video:");
                if (startIndex < 0)
                {
                    return null;
                }
                input = input.Substring(startIndex + 6);
                if (string.IsNullOrEmpty(input))
                {
                    return null;
                }
                input = input.Trim('\r', '\n');
                input = input.Replace(" ", "");
                string[] param = input.Split(',');
                if (param.Length < 5)
                {
                    return null;
                }
                VideoInfo info = new VideoInfo();
                info.Encode = param[0];
                info.Format = param[1];
                //分辨率
                string hw = param[2];
                if (hw.Contains("x"))
                {
                    Match match = Regex.Match(hw, @"(\d+x\d+)");
                    if (match.Success && !string.IsNullOrEmpty(match.Value))
                    {
                        string[] hws = match.Value.Split('x');
                        if (hws.Length == 2 && int.TryParse(hws[0], out int width) && int.TryParse(hws[1], out int height))
                        {
                            info.Width = width;
                            info.Height = height;
                        }
                    }
                }
                info.FPS = param[4].Replace("fps", "", StringComparison.OrdinalIgnoreCase);

                return info;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
