using App.Data.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Config
{
    public class AllowExtensionNode
    {
        public FileType Type { get; set; }

        public List<string> Values { get; set; }
    }
}
