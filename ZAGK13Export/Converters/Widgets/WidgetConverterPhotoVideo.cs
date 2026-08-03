//using CMS.DocumentEngine;
//using Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ZAGK13Export.Converters.Widgets
//{
//    class WidgetConverterPhotoVideo : IWidgetConverter
//    {
//        public string Type => "custom.PartialPhotoVideo";
//        public string TargetType => "Custom.Components.Widgets.PhotoVideo";
//        private readonly FieldConverters _fieldConverters;

//        public WidgetConverterPhotoVideo(FieldConverters fieldConverters)
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
//                                { "titleH1", page.GetValue<bool>("TitleH1", false) },
//                                { "title", page.GetValue<string>("Title", "") },
//                                { "text", page.GetValue<string>("Text", "") },
//                                { "ctas", _fieldConverters.ConvertCtas(page.GetValue<string>("Ctas", "")) },
//                                { "mediaType", page.GetValue<string>("MediaType", "") },
//                                { "image", _fieldConverters.ConvertMediaItemReference(page.GetValue("Image", "")) },
//                                { "imageAlt", page.GetValue<string>("ImageAlt", "") },
//                                { "videoSource", page.GetValue<string>("VideoSource", "") },
//                                { "videoTitle", page.GetValue<string>("VideoTitle", "") },
//                                { "videoTranscript", page.GetValue<string>("VideoTranscript", "") }
//                            },
//                            fieldIdentifiers = new Dictionary<string, object>
//                            {
//                                { "guid", Guid.NewGuid().ToString() },
//                                { "ariaLabel", Guid.NewGuid().ToString() },
//                                { "titleH1", Guid.NewGuid().ToString() },
//                                { "title", Guid.NewGuid().ToString() },
//                                { "text", Guid.NewGuid().ToString() },
//                                { "ctas", Guid.NewGuid().ToString() },
//                                { "mediaType", Guid.NewGuid().ToString() },
//                                { "image", Guid.NewGuid().ToString() },
//                                { "imageAlt", Guid.NewGuid().ToString() },
//                                { "videoSource", Guid.NewGuid().ToString() },
//                                { "videoTitle", Guid.NewGuid().ToString() },
//                                { "videoTranscript", Guid.NewGuid().ToString() }
//                            }
//                        }
//                    }
//                }
//            };

//            return newWidget;
//        }
//    }
//}
