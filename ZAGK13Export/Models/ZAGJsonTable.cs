using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Models
{
    class ZAGJsonTable
    {
        public List<ZAGJsonTableRows> rows { get; set; } = new List<ZAGJsonTableRows>();
    }

    class ZAGJsonTableRows
    {
        public string guid { get; set; }
        public string type { get; set; }
        public List<ZAGJsonTableField> fields { get; set; }
    }

    class ZAGJsonTableField
    {
        public string name { get; set; }
        public string slug { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public List<string> options { get; set; }
        public bool visible { get; set; }
        public bool tableRowSpan { get; set; }
        public bool tableColSpan { get; set; }
        public int tableRowSpanCount { get; set; }
        public int tableColSpanCount { get; set; }
    }
}
