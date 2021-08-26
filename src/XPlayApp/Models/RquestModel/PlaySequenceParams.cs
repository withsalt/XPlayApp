using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XPlayApp.Models.RquestModel
{
    public class SequenceItem
    {
        public int Id { get; set; }

        public string Type { get; set; }
    }

    public class PlaySequenceParams
    {
        public List<SequenceItem> Items { get; set; }
    }
}
