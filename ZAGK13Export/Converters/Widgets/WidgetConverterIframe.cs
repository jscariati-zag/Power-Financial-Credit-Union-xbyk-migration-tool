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
    class WidgetConverterIframe : IWidgetConverter
    {
        public string Type => "custom.PartialIframe";
        public string TargetType => "Custom.Components.Widgets.Iframe";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterIframe(FieldConverters fieldConverters)
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
                                { "source", page.GetValue<string>("IframeSource", "") },
                                { "height", page.GetValue<string>("IframeHeight", "") },
                                { "transcriptURL", page.GetValue<string>("IframeTranscript", "") },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "text", Guid.NewGuid().ToString() },
                                { "source", Guid.NewGuid().ToString() },
                                { "height", Guid.NewGuid().ToString() },
                                { "transcriptURL", Guid.NewGuid().ToString() },
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
