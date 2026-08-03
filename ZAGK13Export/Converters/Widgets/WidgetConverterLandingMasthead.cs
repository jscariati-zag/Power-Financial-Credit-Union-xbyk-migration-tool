//using CMS.DocumentEngine;
//using Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ZAGK13Export.Converters.Widgets
//{
//    class WidgetConverterLandingMasthead : IWidgetConverter
//    {
//        public string Type => "custom.PartialLandingMasthead";
//        public string TargetType => "Custom.Components.Widgets.LandingMasthead";
//        private readonly FieldConverters _fieldConverters;

//        public WidgetConverterLandingMasthead(FieldConverters fieldConverters)
//        {
//            _fieldConverters = fieldConverters;
//        }

//        public Widget Convert(TreeNode page)
//        {
//            var newWidget = new Widget
//            {
//                identifier = Guid.NewGuid().ToString(),
//                type = TargetType,
//                variants = new List<Variant>
//                {
//                    { new Variant
//                        {
//                            identifier = Guid.NewGuid().ToString(),
//                            properties = new Dictionary<string, object>
//                            {
//                                { "guid", Guid.NewGuid().ToString() },
//                                { "ariaLabel", page.DocumentName },
//                                { "title", page.GetValue<string>("Title", "") },
//                                { "text", page.GetValue<string>("Text", "") },
//                                { "ctas", _fieldConverters.ConvertCtas(page.GetValue<string>("Ctas", "")) },
//                                { "image", _fieldConverters.ConvertMediaItemReference(page.GetValue("Image", "")) }
//                            },
//                            fieldIdentifiers = new Dictionary<string, object>
//                            {
//                                { "guid", Guid.NewGuid().ToString() },
//                                { "ariaLabel", Guid.NewGuid().ToString() },
//                                { "title", Guid.NewGuid().ToString() },
//                                { "text", Guid.NewGuid().ToString() },
//                                { "ctas", Guid.NewGuid().ToString() },
//                                { "image", Guid.NewGuid().ToString() }
//                            }
//                        }
//                    }
//                }
//            };

//            return newWidget;
//        }
//    }
//}
