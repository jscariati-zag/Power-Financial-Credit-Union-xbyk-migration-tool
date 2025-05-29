using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters.Widgets
{
    class WidgetConverterBios : IWidgetConverter
    {
        public string Type => "custom.PartialBios";
        public string TargetType => "Custom.Components.Widgets.Bios";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterBios(FieldConverters fieldConverters)
        {
            _fieldConverters = fieldConverters;
        }

        public Widget Convert(TreeNode page)
        {
            var bios = page.Parent.Parent.Children
                            .Where(c => c.ClassName == "custom.PageBio")
                            .OrderBy(c => c.NodeOrder)
                            .Select(r => new PageReference
                            {
                                OldGuid = r.NodeGUID
                            }).ToList();

            var newWidget = new Widget
            {
                identifier = Guid.NewGuid().ToString(),
                type = TargetType,
                variants = new List<Variant>
                {
                    { new Variant
                        {
                            identifier = Guid.NewGuid().ToString(),
                            properties = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "title", page.GetValue<string>("Title", "") },
                                { "text", page.GetValue<string>("Text", "") },
                                { "bios", bios },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "text", Guid.NewGuid().ToString() },
                                { "bios", Guid.NewGuid().ToString() },
                                { "anchorText", Guid.NewGuid().ToString() }
                            }
                        }
                    }
                }
            };

            return newWidget;
        }
    }
}
