using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using XPlayApp.Services.XPlay.Enum;

namespace XPlayApp.Services.XPlay.Model
{
    public class XPlayCommand: IXPlayCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CommandType type { get; set; }

        /// <summary>
        /// 开始时间(默认：-1，立即播放，本地毫秒时间戳)【非必填】
        /// </summary>
        public int start { get; set; } = -1;

        /// <summary>
        /// 素材类型(video、pic、camera、gif、qrcode、text、scroll）【必填】
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SourceType libName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public XPlayCommandData @params { get; set; }

        public List<XPlayCommandDeps> deps = new List<XPlayCommandDeps>();
    }
}
