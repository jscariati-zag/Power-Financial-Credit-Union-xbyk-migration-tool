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
    class ContentItemConverterComparisonPersonalSavingsProduct : IContentItemConverter
    {
        public string Type => "custom.PartialComparisonPersonalSavingsProduct";
        public string TargetType => "Custom.Reusable_ComparisonPersonalSavingsProduct";
        public string FolderDisplayName => "Comparison Personal Savings Products";
        public string FolderName => "ComparisonPersonalSavingsProducts";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public ContentItemConverterComparisonPersonalSavingsProduct(IConfiguration config, FieldConverters fieldConverters)
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
                    { "Ctas", _fieldConverters.ConvertCtas(page.GetValue<string>("Ctas", "")) },
                    { "BalanceToEarnInterest", page.GetValue<string>("BalanceToEarnInterest", "") },
                    { "WithdrawalFee", page.GetValue<string>("WithdrawalFee", "") },
                    { "ServiceCharge", page.GetValue<string>("ServiceCharge", "") },
                }
            };

            return newContentItem;
        }
    }
}
