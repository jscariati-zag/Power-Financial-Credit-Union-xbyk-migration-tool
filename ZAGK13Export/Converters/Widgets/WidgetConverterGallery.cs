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
    class WidgetConverterGallery : IWidgetConverter
    {
        public string Type => "custom.PartialGallery";
        public string TargetType => "Custom.Components.Widgets.Gallery";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterGallery(FieldConverters fieldConverters)
        {
            _fieldConverters = fieldConverters;
        }

        public Widget Convert(TreeNode page)
        {
            var galleryItems = page.Children
                            .Where(c => c.ClassName == "custom.PartialGalleryItem")
                            .OrderBy(c => c.NodeOrder)
                            .Select(r => new ContentReference
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
                                { "style", page.GetValue<string>("Style", "") },
                                { "title", page.GetValue<string>("Title", "") },
                                { "text", page.GetValue<string>("Text", "") },
                                { "galleryItems", galleryItems },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "style", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "text", Guid.NewGuid().ToString() },
                                { "galleryItems", Guid.NewGuid().ToString() },
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
