using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.FFmpeg.Models
{
    public class VideoInfo
    {
        public int Duration { get; set; } = 0;

        public string Encode { get; set; }

        public string Format { get; set; }

        public string FPS { get; set; }

        public int Height { get; set; } = 0;

        public int Width { get; set; } = 0;
    }
}
