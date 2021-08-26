using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using XPlayApp.Services.XPlay.Enum;

namespace XPlayApp.Services.XPlay.Model
{
    public class XPlayCommandData
    {
        /// <summary>
        /// 层(支持多层播放，层数越小画面越靠前)【必填】
        /// </summary>
        public int zIndex { get; set; } = 10;

        /// <summary>
        /// 素材路径【必填】
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// 距左像素(X轴)【非必填】
        /// </summary>
        public int left { get; set; }

        /// <summary>
        /// 距顶像素(Y轴)【非必填】
        /// </summary>
        public int top { get; set; }

        /// <summary>
        /// 宽(素材显示的宽，非素材原始尺寸，支持缩放拉伸)【必填】
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// 高(素材显示的高，非素材原始尺寸，支持缩放拉伸)【必填】
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// 屏幕模式(横屏：landscape，竖屏：portrait，默认横屏)【非必填】
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ScreenMode screen_mode { get; set; }

        /// <summary>
        /// 旋转角度(横屏角度：0、180，竖屏角度：90、270，默认横屏)【非必填】
        /// </summary>
        public int screen_rotate { get; set; } = 0;

        [JsonConverter(typeof(StringEnumConverter))]
        public ToastType toast_type { get; set; }

        public string content { get; set; }

        public int duration { get; set; }

        public string color { get; set; }

        public string bgcolor { get; set; }

        public int font_size { get; set; }

        public string align { get; set; }

        public string style { get; set; }
    }
}
