//using CMS.DocumentEngine;
//using Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Org.BouncyCastle.Math.EC.ECCurve;

//namespace ZAGK13Export.Converters.Widgets
//{
//    class WidgetConverterSearchResults : IWidgetConverter
//    {
//        public string Type => "custom.PartialSearchResults";
//        public string TargetType => "Custom.Components.Widgets.Search";
//        private readonly FieldConverters _fieldConverters;

//        public WidgetConverterSearchResults(FieldConverters fieldConverters)
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
//                                { "index", "Site" }
//                            },
//                            fieldIdentifiers = new Dictionary<string, object>
//                            {
//                                { "guid", Guid.NewGuid().ToString() },
//                                { "ariaLabel", Guid.NewGuid().ToString() },
//                                { "title", Guid.NewGuid().ToString() },
//                                { "text", Guid.NewGuid().ToString() },
//                                { "index", Guid.NewGuid().ToString() }
//                            }
//                        }
//                    }
//                }
//            };

//            return newWidget;
//        }
//    }
//}
