using System;
using System.Collections.Generic;
using System.Text;

namespace App.Config
{
    public class AppSettingsNode
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// 是否初始化数据库
        /// </summary>
        public bool IsSeedDatabase { get; set; }

        public string PasswdSalt { get; set; }

        public string WebTokenPassword { get; set; }

        public int WebTokenSaveTime { get; set; }

        public List<AllowExtensionNode> AllowExtensions { get; set; }

        public string XPlayServer { get; set; }

        public ScreenMode Screen { get; set; }
    }

    public class ScreenMode
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }
}
