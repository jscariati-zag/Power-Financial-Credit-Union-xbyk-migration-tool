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
    class ContentItemConverterAlert : IContentItemConverter
    {
        public string Type => "custom.Alert";
        public string TargetType => "Custom.Reusable_Alert";
        public string FolderDisplayName => "Alerts";
        public string FolderName => "Alerts";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public ContentItemConverterAlert(IConfiguration config, FieldConverters fieldConverters)
        {
            _config = config;
            _fieldConverters = fieldConverters;
        }

        public ContentItem Convert(TreeNode page)
        {
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
                    { "Name", page.DocumentName }
                }
            };

            return newContentItem;
        }
    }
}
