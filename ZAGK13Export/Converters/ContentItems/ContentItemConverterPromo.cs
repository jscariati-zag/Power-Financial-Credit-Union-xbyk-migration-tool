using CMS.DocumentEngine;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters.ContentItems
{
    class ContentItemConverterPromo : IContentItemConverter
    {
        public string Type => "custom.PartialPromo";
        public string TargetType => "Custom.Reusable_Promo";
        public string FolderDisplayName => "Promos";
        public string FolderName => "Promos";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public ContentItemConverterPromo(IConfiguration config, FieldConverters fieldConverters)
        {
            _config = config;
            _fieldConverters = fieldConverters;
        }

        public ContentItem? Convert(TreeNode page)
        {
            if (page.IsLink) { return null; }
            var newContentItem = new ContentItem
            {
                OldGuid = page.NodeGUID,
                DisplayName = page.DocumentName,
                ContentType = TargetType,
                Language = _config.GetValue<string>("TargetLanguage"),
                Published = page.IsPublished,
                FolderName = FolderName,
                ItemData = new Dictionary<string, object>
                {
                    { "Title", page.GetValue<string>("Title", "") },
                    { "Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("Image", "")) },
                    { "CtaTitle", page.GetValue<string>("Cta", "") },
                    { "Cta", _fieldConverters.ConvertCtas($"|{page.GetValue<string>("Url", "").TrimStart('~')}|{page.GetValue<string>("Target", "")}|") },
                }
            };

            return newContentItem;
        }
    }
}
