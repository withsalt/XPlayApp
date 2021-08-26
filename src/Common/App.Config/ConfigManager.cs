using App.Util.Json;
using System;
using System.IO;

namespace App.Config
{
    public class ConfigManager
    {
        private static ConfigManager _manager = null;

        public static ConfigManager Instance
        {
            get
            {
                if (_manager == null)
                {
                    throw new Exception("Please load config at first.");
                }
                return _manager;
            }
        }

        public void Load()
        {
            try
            {
                string configPath = $"{AppContext.BaseDirectory}appsettings.json";
                if (!File.Exists(configPath))
                {
                    throw new Exception("Config file appsettings.json is not exist.");
                }
                string loadConfigString = File.ReadAllText(configPath);
                if (string.IsNullOrEmpty(loadConfigString))
                {
                    throw new Exception("Appsettings.json content is null.");
                }
                _manager = JsonUtil.DeserializeJsonToObject<ConfigManager>(loadConfigString);
                if (_manager == null)
                {
                    throw new Exception("Appsettings.json load failed.");
                }
                //set value
                this.ConnectionStrings = _manager.ConnectionStrings;
                this.Logging = _manager.Logging;
                this.AllowedHosts = _manager.AllowedHosts;
                this.AppSettings = _manager.AppSettings;
            }
            catch (Exception ex)
            {
                throw new Exception($"Load config failed. {ex.Message}");
            }
        }

        public bool Save()
        {
            try
            {
                string json = JsonUtil.SerializeObject(this);
                if (string.IsNullOrEmpty(json))
                {
                    throw new Exception("Can not serialize object to json.");
                }
                string configPath = $"{AppContext.BaseDirectory}appsettings.json";
                File.WriteAllText(json,configPath);
                if (!File.Exists(configPath))
                {
                    throw new Exception("Config file appsettings.json writen failed.");
                }
                return true;
            }
            catch(Exception ex)
            {
                throw new Exception($"Save config failed. {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public LoggingNode Logging { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AllowedHosts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AppSettingsNode AppSettings { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public ConnectionStringsNode ConnectionStrings { get; set; }
    }
}
