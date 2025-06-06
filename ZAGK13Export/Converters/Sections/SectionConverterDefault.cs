using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters.Widgets
{
    class SectionConverterDefault : ISectionConverter
    {
        public string Type => "";
        public string TargetType => "Custom.Components.Sections.Default";
        public bool IsDefault => true;
        private readonly FieldConverters _fieldConverters;

        public SectionConverterDefault(FieldConverters fieldConverters)
        {
            _fieldConverters = fieldConverters;
        }

        public Section Convert(TreeNode page)
        {
            var sectionGuid = Guid.NewGuid().ToString();
            var newSection = new Section
            {
                identifier = Guid.NewGuid().ToString(),
                type = TargetType,
                properties = new Dictionary<string, object>
                {
                    { "guid", sectionGuid }
                },
                zones = new List<Zone>
                {
                    { new Zone()
                        {
                            identifier = Guid.NewGuid().ToString(),
                            name = "Section_" + sectionGuid
                        }
                    }
                },
                fieldIdentifiers = new Dictionary<string, object>
                {
                    { "background", Guid.NewGuid().ToString() },
                    { "guid", Guid.NewGuid().ToString() }
                }
            };

            return newSection;
        }
    }
}
