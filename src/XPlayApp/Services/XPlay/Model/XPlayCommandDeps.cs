using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XPlayApp.Services.XPlay.Enum;

namespace XPlayApp.Services.XPlay.Model
{
    public class XPlayCommandDeps
    {
        public int duration { get; set; } = -1;

        public string path { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SourceType type { get; set; }
    }
}
