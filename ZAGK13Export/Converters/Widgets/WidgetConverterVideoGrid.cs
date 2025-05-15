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
    class WidgetConverterVideoGrid : IWidgetConverter
    {
        public string Type => "custom.PartialVideoGrid";
        public string TargetType => "Custom.Components.Widgets.VideoGrid";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterVideoGrid(FieldConverters fieldConverters)
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
                                { "key", page.GetValue<string>("Key", "") },
                                { "title", page.GetValue<string>("Title", "") },
                                { "skipCount", page.GetValue<int>("SkipCount", 1) },
                                { "initialRows", page.GetValue<int>("InitialRows", 2) },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "key", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "skipCount", Guid.NewGuid().ToString() },
                                { "initialRows", Guid.NewGuid().ToString() },
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
