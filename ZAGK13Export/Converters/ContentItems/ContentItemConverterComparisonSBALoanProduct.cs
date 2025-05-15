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
    class ContentItemConverterComparisonSBALoanProduct : IContentItemConverter
    {
        public string Type => "custom.PartialComparisonSbaLoanProduct";
        public string TargetType => "Custom.Reusable_ComparisonSbaLoanProduct";
        public string FolderDisplayName => "Comparison SBA Loans Products";
        public string FolderName => "ComparisonSBALoanProducts";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public ContentItemConverterComparisonSBALoanProduct(IConfiguration config, FieldConverters fieldConverters)
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
                    { "MaxLoanSize", page.GetValue<string>("MaxLoanSize", "") },
                    { "Term", page.GetValue<string>("Term", "") },
                    { "Use", page.GetValue<string>("Use", "") },
                    { "Rates", page.GetValue<string>("Rates", "") },
                }
            };

            return newContentItem;
        }
    }
}
