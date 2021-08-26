using App.Config;
using App.Data.Entity.System;
using App.Data.Enum;
using App.Util.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XPlayApp.Services.XPlay.Client;
using XPlayApp.Services.XPlay.Enum;
using XPlayApp.Services.XPlay.Model;
using TcpClient = XPlayApp.Services.XPlay.Client.TcpClient;

namespace XPlayApp.Services.XPlay
{
    public enum SequenceStatus
    {
        Playing,
        Stoped,
    }

    public class XPlayService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<XPlayService> _logger;
        private readonly ConfigManager _config;
        private readonly TcpClient _client;
        private readonly IServer _server;

        private Queue<Material> _sequence = new Queue<Material>();
        private readonly static object _locker = new object();
        private CancellationTokenSource tokenSource = null;
        private Task _mainTask = null;

        public bool ConnectStatus
        {
            get
            {
                return _client.Status;
            }
        }

        public SequenceStatus SequenceStatus
        {
            get; private set;
        }

        public XPlayService(ILogger<XPlayService> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IServer server)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<XPlayService>));
            this._hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._server = server ?? throw new ArgumentNullException(nameof(IServer));

            _client = new TcpClient(_config.AppSettings.XPlayServer, 8700)
                .WithAutoReconnect()
                .WithConnected((sender, e) =>
                {
                    XplayConnected();
                })
                .WithDisconnected((sender, e) =>
                {
                    _logger.LogWarning($"Disconnect with xplay servier.");
                })
                .WithError((sender, e) =>
                {
                    if (e.IsSocketError)
                    {
                        //_logger.LogWarning($"Tcp client socket error.");
                    }
                    else
                    {
                        if (e.Exception == null)
                        {
                            return;
                        }
                        _logger.LogWarning($"Tcp client warning, {e.Exception.Message}", e.Exception);
                    }
                })
                .WithReceived((sender, e) =>
                {
                    DataReceived(e.MessageString);
                });

            this.SequenceStatus = SequenceStatus.Stoped;
        }

        public async Task<bool> Connect()
        {
            try
            {
                if (_client.Status)
                {
                    return await Task.FromResult(true);
                }
                _client.ConnectAsync();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return await Task.FromResult(false);
            }
        }

        public async Task<CommandResult> Excute(IXPlayCommand data)
        {
            if (!_client.Status)
            {
                throw new Exception("Xplay server not connected.");
            }
            StringBuilder cmd = new StringBuilder();
            cmd.Append(JsonUtil.SerializeObject(data));
            cmd.Append("\n#End\n");
            try
            {
                return await Task.Run(() =>
                {
                    string resultStr = _client.SendAndWait(cmd.ToString());
                    if (string.IsNullOrEmpty(resultStr))
                    {
                        return new CommandResult()
                        {
                            Ok = false,
                            data = "返回数据为空。"
                        };
                    }
                    resultStr = resultStr.Trim('\r', '\n')
                        .Replace("#End", "", StringComparison.OrdinalIgnoreCase);

                    CommandResult result = JsonUtil.DeserializeJsonToObject<CommandResult>(resultStr);
                    if (result == null)
                    {
                        _logger.LogWarning($"Deserialize result string failed. result data:{resultStr}");
                        return new CommandResult()
                        {
                            Ok = false,
                            data = "解析返回数据失败。"
                        };
                    }
                    return result;
                });
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    _logger.LogWarning($"Send command data error, {ex.Message}", ex);
                    return new CommandResult()
                    {
                        Ok = false,
                        data = $"系统错误，{ex.Message}"
                    };
                }
                else
                {
                    return new CommandResult()
                    {
                        Ok = false,
                        data = $"系统错误，未知原因。"
                    };
                }
            }
        }

        #region Play

        public async Task<(bool, string)> PlayVideo(Material material, bool isSequence = false)
        {
            double width = _config.AppSettings.Screen.Height;
            double height = _config.AppSettings.Screen.Height;
            if (material.Width != 0 && material.Height != 0)
            {
                double multiple = _config.AppSettings.Screen.Height / material.Height;
                width = material.Width * multiple;
                if (width > _config.AppSettings.Screen.Width)
                {
                    width = _config.AppSettings.Screen.Width;
                }
            }
            XPlayCommand playCommand = new XPlayCommand()
            {
                type = CommandType.play,
                libName = SourceType.video,
                @params = new XPlayCommandData()
                {
                    path = material.Path,
                    width = (int)width,
                    height = (int)height,
                    left = (int)((_config.AppSettings.Screen.Width - width) / 2),
                    top = 0
                }
            };
            if (this.SequenceStatus == SequenceStatus.Playing && !isSequence)
            {
                if (!await StopSequence())
                {
                    return (false, "停止正在播放的序列失败。");
                }
            }
            CommandResult result = await Excute(playCommand);
            if (result.Ok)
            {
                return (true, null);
            }
            else
            {
                return (false, result.data.ToString());
            }
        }

        public async Task<(bool, string)> PlayAudio(Material material, bool isSequence = false)
        {
            XPlayCommand playCommand = new XPlayCommand()
            {
                type = CommandType.play,
                libName = SourceType.video,
                @params = new XPlayCommandData()
                {
                    path = material.Path,
                    width = _config.AppSettings.Screen.Height,
                    height = _config.AppSettings.Screen.Height,
                    left = (int)((_config.AppSettings.Screen.Width - _config.AppSettings.Screen.Height) / 2),
                }
            };
            if (this.SequenceStatus == SequenceStatus.Playing && !isSequence)
            {
                if (!await StopSequence())
                {
                    return (false, "停止正在播放的序列失败。");
                }
            }
            CommandResult result = await Excute(playCommand);
            if (result.Ok)
            {
                return (true, null);
            }
            else
            {
                return (false, result.data.ToString());
            }
        }

        public async Task<(bool, string)> PlayImage(Material material, bool isSequence = false)
        {
            string ext = Path.GetExtension(material.Path);
            SourceType sourceType = SourceType.pic;
            if (ext.Contains(".gif", StringComparison.OrdinalIgnoreCase))
            {
                sourceType = SourceType.gif;
            }
            double width = _config.AppSettings.Screen.Height;
            double height = _config.AppSettings.Screen.Height;
            if (material.Width != 0 && material.Height != 0)
            {
                double multiple = _config.AppSettings.Screen.Height / material.Height;
                width = material.Width * multiple;
                if (width > _config.AppSettings.Screen.Width)
                {
                    width = _config.AppSettings.Screen.Width;
                }
            }

            XPlayCommand playCommand = new XPlayCommand()
            {
                type = CommandType.play,
                libName = sourceType,
                @params = new XPlayCommandData()
                {
                    path = material.Path,
                    width = (int)width,
                    height = (int)height,
                    left = (int)((_config.AppSettings.Screen.Width - width) / 2),
                    top = 0,
                    duration = 0,
                }
            };
            if (this.SequenceStatus == SequenceStatus.Playing && !isSequence)
            {
                if (!await StopSequence())
                {
                    return (false, "停止正在播放的序列失败。");
                }
            }
            CommandResult result = await Excute(playCommand);
            if (result.Ok)
            {
                return (true, null);
            }
            else
            {
                return (false, result.data.ToString());
            }
        }

        public async Task<(bool, string)> PlaySequence(List<Material> playList)
        {
            if (playList == null || playList.Count == 0)
            {
                return (false, "播放列表为空");
            }
            if (this.SequenceStatus == SequenceStatus.Playing)
            {
                if (!await StopSequence())
                {
                    return (false, "停止正在播放的序列失败。");
                }
            }
            if (tokenSource == null)
            {
                tokenSource = new CancellationTokenSource();
            }
            else
            {
                tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
            }
            foreach (var item in playList)
            {
                _sequence.Enqueue(item);
            }
            if (_mainTask != null)
            {
                try
                {
                    _mainTask.Dispose();
                }
                catch { }
                finally
                {
                    _mainTask = null;
                }
            }
            _mainTask = new Task(async () =>
            {
                this.SequenceStatus = SequenceStatus.Playing;
                while (_sequence.Count > 0 && this.SequenceStatus == SequenceStatus.Playing)
                {
                    Material material = null;
                    lock (_locker)
                    {
                        material = _sequence.Dequeue();
                    }
                    (bool, string) result;
                    int playTime = 0;
                    switch (material.FileType)
                    {
                        case FileType.Image:
                            result = await PlayImage(material, true);
                            playTime = 60;
                            break;
                        case FileType.Video:
                            if (material.Duration == null || material.Duration.Value == 0)
                            {
                                result = (false, "获取文件时长失败。");
                            }
                            else
                            {
                                result = await PlayVideo(material, true);
                                playTime = material.Duration.HasValue ? material.Duration.Value : 0;
                            }
                            break;
                        case FileType.Music:
                            if (material.Duration == null || material.Duration.Value == 0)
                            {
                                result = (false, "获取文件时长失败。");
                            }
                            else
                            {
                                result = await PlayAudio(material, true);
                                playTime = material.Duration.HasValue ? material.Duration.Value : 0;
                            }
                            break;
                        default:
                            result = (false, $"未知的播放文件类型：{material.FileType}");
                            break;
                    }
                    if (!result.Item1)
                    {
                        _logger.LogWarning($"序列内容“{material.FileOldName}”播放失败，{result.Item2}");
                    }
                    else
                    {
                        _logger.LogInformation($"正在播放序列内容“{material.FileOldName}”。");
                        try
                        {
                            await Task.Delay(playTime * 1000, tokenSource.Token);
                        }
                        catch
                        {

                        }
                    }
                }
                if (this.SequenceStatus != SequenceStatus.Stoped)
                {
                    lock (_locker)
                    {
                        if (this.SequenceStatus != SequenceStatus.Stoped)
                        {
                            _sequence.Clear();
                            this.SequenceStatus = SequenceStatus.Stoped;
                        }
                    }
                }
            }, tokenSource.Token);
            _mainTask.Start();
            return (true, null);
        }

        #endregion

        #region private

        private void DataReceived(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            _logger.LogInformation($"Received:" + data);
        }

        private void XplayConnected()
        {
            _logger.LogInformation($"Connect with xplay servier.");
            //延迟100ms
            Thread.Sleep(100);
            //load ips
            ShowLocalIpAddress();
        }

        private async void ShowLocalIpAddress()
        {
            try
            {
                List<UnicastIPAddressInformation> ipInfos = NetworkInterface.GetAllNetworkInterfaces()
                    .Select(p => p.GetIPProperties())
                    .SelectMany(p => p.UnicastAddresses)
                    .Where(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))
                    .ToList();
                if (ipInfos == null || ipInfos.Count == 0)
                {
                    return;
                }
                List<string> ips = new List<string>();
                foreach (var item in ipInfos)
                {
                    if (item.Address == null)
                    {
                        continue;
                    }
                    string ip = item.Address.ToString();
                    if (ip.StartsWith("0.0.0") || ip.StartsWith("169") || ip.StartsWith("172") || ip.Equals("10.0.0.1"))
                    {
                        continue;
                    }
                    ips.Add(ip);
                }
                if (ips == null || ips.Count == 0)
                {
                    ips.Add("请先联网！");
                }
                List<string> endpoints = GetEndpoint(ips);
                int top = _config.AppSettings.Screen.Height / 2 - 250;
                if(top <= 0)
                {
                    top = 30;
                }
                int left = _config.AppSettings.Screen.Width / 2 - 200;
                if(left <= 0)
                {
                    left = 60;
                }
                XPlayCommand command = new XPlayCommand()
                {
                    type = CommandType.play,
                    libName = SourceType.text,
                    @params = new XPlayCommandData()
                    {
                        top = top,
                        left = left,
                        width = _config.AppSettings.Screen.Width,
                        height = 400,
                        color = "rgba(255, 0, 0, 100%)",
                        bgcolor = "rgba(0, 0, 0, 0%)",
                        font_size = 30,
                        align = "center",
                        style = "bold",
                        content = "访问地址：\n" + string.Join("\n", endpoints),
                    }
                };
                CommandResult result = await Excute(command);
                if (!result.Ok)
                {
                    throw new Exception("Excute toast command failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Show ip info in screen failed. {ex.Message}", ex);
            }
        }

        private async Task<bool> StopSequence()
        {
            try
            {
                if (this.SequenceStatus == SequenceStatus.Stoped)
                {
                    if (_mainTask != null)
                    {
                        try
                        {
                            _mainTask.Dispose();
                        }
                        catch { }
                        finally
                        {
                            _mainTask = null;
                        }
                    }
                    return true;
                }
                if (tokenSource == null)
                {
                    return false;
                }
                if (_mainTask == null)
                {
                    return false;
                }
                if (this.SequenceStatus != SequenceStatus.Stoped)
                {
                    lock (_locker)
                    {
                        if (this.SequenceStatus != SequenceStatus.Stoped)
                        {
                            this.SequenceStatus = SequenceStatus.Stoped;
                        }
                    }
                }

                tokenSource.Cancel();
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (_mainTask.Status != TaskStatus.RanToCompletion && _mainTask.Status != TaskStatus.Canceled)
                {
                    if (sw.ElapsedMilliseconds > 1000)
                    {
                        sw.Stop();
                        return false;
                    }
                    await Task.Delay(5);
                }
                _mainTask.Dispose();
                _mainTask = null;
                lock (_locker)
                {
                    if (tokenSource != null)
                    {
                        tokenSource.Dispose();
                        tokenSource = new CancellationTokenSource();
                    }

                    _sequence.Clear();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Set cancel status failed, {ex.Message}", ex);
                return false;
            }
        }

        private List<string> GetEndpoint(List<string> ipAddress)
        {
            string endpoint = null;
            var address = _server.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (address == null || address.Length == 0)
            {
                throw new Exception("Can not get current app endpoint.");
            }
            if (address.Length > 1)
            {
                foreach (var item in address)
                {
                    if (item.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        endpoint = item;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(endpoint))
                {
                    endpoint = address[0];
                }
            }
            else
            {
                endpoint = address[0];
            }
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Can not get current app endpoint.");
            }
            List<string> result = new List<string>();
            var uri = Regex.Replace(endpoint, @"^(?<scheme>https?):\/\/((\+)|(\*)|\[::\]|(0.0.0.0))(?=[\:\/]|$)", "${scheme}://localhost");
            foreach(var item in ipAddress)
            {
                Uri httpEndpoint = new Uri(uri, UriKind.Absolute);
                string newUri = new UriBuilder(httpEndpoint.Scheme, item, httpEndpoint.Port).ToString().TrimEnd('/');
                result.Add(newUri);
            }
            return result;
        }

        #endregion
    }
}
