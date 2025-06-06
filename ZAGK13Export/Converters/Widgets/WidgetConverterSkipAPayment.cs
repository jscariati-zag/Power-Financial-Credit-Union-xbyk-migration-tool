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
    class WidgetConverterSkipAPayment : IWidgetConverter
    {
        public string Type => "custom.PartialSkipAPayment";
        public string TargetType => "Custom.Components.Widgets.SkipAPayment";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterSkipAPayment(FieldConverters fieldConverters)
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
                                { "title", page.GetValue<string>("Title", "") },
                                { "defermentOptionYear", page.GetValue<string>("Year", "") },
                                { "defermentOptionMonth", page.GetValue<string>("Month", "") },
                                { "skipDefermentOption", page.GetValue<string>("SkipDefermentOption", "") }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "ariaLabel", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "defermentOptionYear", Guid.NewGuid().ToString() },
                                { "defermentOptionMonth", Guid.NewGuid().ToString() },
                                { "skipDefermentOption", Guid.NewGuid().ToString() }
                            }
                        }
                    }
                }
            };

            return newWidget;
        }
    }
}
