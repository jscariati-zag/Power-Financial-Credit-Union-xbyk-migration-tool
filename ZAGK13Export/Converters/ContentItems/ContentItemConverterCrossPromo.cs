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
    class ContentItemConverterCrossPromo : IContentItemConverter
    {
        public string Type => "custom.PartialCrossPromo";
        public string TargetType => "Custom.Reusable_CrossPromo";
        public string FolderDisplayName => "Cross Promos";
        public string FolderName => "CrossPromos";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public ContentItemConverterCrossPromo(IConfiguration config, FieldConverters fieldConverters)
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
                    { "Text", page.GetValue<string>("Text", "") },
                    { "Ctas", _fieldConverters.ConvertCtas(page.GetValue<string>("Ctas", "")) },
                    { "Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("Image", "")) },
                }
            };

            return newContentItem;
        }
    }
}
