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
    class WidgetConverterDinkytown : IWidgetConverter
    {
        public string Type => "custom.PartialDinkytown";
        public string TargetType => "Custom.Components.Widgets.Dinkytown";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterDinkytown(FieldConverters fieldConverters)
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
                                { "title", page.GetValue<string>("Title", "") },
                                { "text", page.GetValue<string>("Text", "") },
                                { "dinkytownCode", page.GetValue<string>("DinkytownCode", "") },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "text", Guid.NewGuid().ToString() },
                                { "dinkytownCode", Guid.NewGuid().ToString() },
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
