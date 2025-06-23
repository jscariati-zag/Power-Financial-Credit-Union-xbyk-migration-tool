using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Models
{
    class K13PageBuilderWidgets
    {
        public List<K13EditableArea> EditableAreas { get; set; }
    }

    public class K13EditableArea
    {
        public string Identifier { get; set; }
        public List<K13Section> Sections { get; set; }
    }

    public class K13Section
    {
        public List<K13Zone> Zones { get; set; }
    }

    public class K13Zone
    {
        public List<K13Widget> Widgets { get; set; }
    }

    public class K13Widget
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
        public List<K13Variant> Variants { get; set; }
    }

    public class K13Variant
    {
        public string Identifier { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
