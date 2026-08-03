//using CMS.DocumentEngine;
//using Common;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ZAGK13Export.Converters.ContentItems
//{
//    class ContentItemConverterCarouselSlide : IContentItemConverter
//    {
//        public string Type => "custom.PartialCarouselSlide";
//        public string TargetType => "Custom.Reusable_CarouselSlide";
//        public string FolderDisplayName => "Carousel Slides";
//        public string FolderName => "CarouselSlides";
//        private readonly IConfiguration _config;
//        private readonly FieldConverters _fieldConverters;

//        public ContentItemConverterCarouselSlide(IConfiguration config, FieldConverters fieldConverters)
//        {
//            _config = config;
//            _fieldConverters = fieldConverters;
//        }

//        public ContentItem? Convert(TreeNode page)
//        {
//            if (page.IsLink) { return null; }
//            var newContentItem = new ContentItem
//            {
//                OldGuid = page.NodeGUID,
//                DisplayName = page.DocumentName,
//                ContentType = TargetType,
//                Language = _config.GetValue<string>("TargetLanguage"),
//                Published = page.IsPublished,
//                FolderName = FolderName,
//                ItemData = new Dictionary<string, object>
//                {
//                    { "Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("Image", "")) },
//                    { "Title", page.GetValue("Title", "") },
//                    { "Subtitle", page.GetValue("Subtitle", "") },
//                    { "Ctas", _fieldConverters.ConvertCtas(page.GetValue("Ctas", "")) },
//                }
//            };

//            return newContentItem;
//        }
//    }
//}
