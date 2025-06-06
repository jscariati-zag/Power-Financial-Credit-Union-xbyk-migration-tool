using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters.Widgets
{
    class WidgetConverterCarousel : IWidgetConverter
    {
        public string Type => "custom.PartialCarousel";
        public string TargetType => "Custom.Components.Widgets.Carousel";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterCarousel(FieldConverters fieldConverters)
        {
            _fieldConverters = fieldConverters;
        }

        public Widget Convert(TreeNode page)
        {
            var carouselSlides = page.Children
                                    .Where(c => c.ClassName == "custom.PartialCarouselSlide")
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
                                { "ariaLabel", page.DocumentName },
                                { "carouselSlides", carouselSlides }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "ariaLabel", Guid.NewGuid().ToString() },
                                { "carouselSlides", Guid.NewGuid().ToString() }
                            }
                        }
                    }
                }
            };

            return newWidget;
        }
    }
}
