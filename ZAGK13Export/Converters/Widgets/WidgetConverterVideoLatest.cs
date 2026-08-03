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
//    class WidgetConverterVideoLatest : IWidgetConverter
//    {
//        public string Type => "custom.PartialVideoLatest";
//        public string TargetType => "Custom.Components.Widgets.VideoLatest";
//        private readonly FieldConverters _fieldConverters;

//        public WidgetConverterVideoLatest(FieldConverters fieldConverters)
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
//                                { "label", page.GetValue<string>("Label", "") }
//                            },
//                            fieldIdentifiers = new Dictionary<string, object>
//                            {
//                                { "guid", Guid.NewGuid().ToString() },
//                                { "ariaLabel", Guid.NewGuid().ToString() },
//                                { "label", Guid.NewGuid().ToString() }
//                            }
//                        }
//                    }
//                }
//            };

//            return newWidget;
//        }
//    }
//}
