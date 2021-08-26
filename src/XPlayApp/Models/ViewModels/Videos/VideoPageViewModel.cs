using App.Data.Entity.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XPlayApp.Models.ViewModels.Videos
{
    public class VideoPageViewModel
    {
        public List<Material> Materials { get; set; } = new List<Material>();
    }
}
