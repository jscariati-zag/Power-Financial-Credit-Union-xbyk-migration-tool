using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters.Widgets
{
    class WidgetConverterRichText : IWidgetConverter
    {
        public string Type => "custom.PartialRichText";
        public string TargetType => "Custom.Components.Widgets.RichText";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterRichText(FieldConverters fieldConverters)
        {
            _fieldConverters = fieldConverters;
        }

        public Widget Convert(TreeNode page)
        {
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
                                { "ariaLabel", page.DocumentName },
                                { "richText", page.GetValue<string>("Text", "") },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "ariaLabel", Guid.NewGuid().ToString() },
                                { "richText", Guid.NewGuid().ToString() },
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
