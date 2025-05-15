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
    class ContentItemConverterIcon : IContentItemConverter
    {
        public string Type => "custom.PartialIcon";
        public string TargetType => "Custom.Reusable_Icon";
        public string FolderDisplayName => "Icons";
        public string FolderName => "Icons";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public ContentItemConverterIcon(IConfiguration config, FieldConverters fieldConverters)
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
                    { "Icon", page.GetValue<string>("IconFa", "") },
                    { "Title", page.GetValue<string>("Title", "") },
                    { "Cta", _fieldConverters.ConvertCtas($"|{page.GetValue<string>("Url", "").TrimStart('~')}|{page.GetValue<string>("Target", "")}|") },
                }
            };

            return newContentItem;
        }
    }
}
