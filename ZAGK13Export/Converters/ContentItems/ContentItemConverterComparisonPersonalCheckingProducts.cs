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
//    class ContentItemConverterComparisonPersonalCheckingProduct : IContentItemConverter
//    {
//        public string Type => "custom.PartialComparisonPersonalCheckingProduct";
//        public string TargetType => "Custom.Reusable_ComparisonPersonalCheckingProduct";
//        public string FolderDisplayName => "Comparison Personal Checking Products";
//        public string FolderName => "ComparisonPersonalCheckingProduct";
//        private readonly IConfiguration _config;
//        private readonly FieldConverters _fieldConverters;

//        public ContentItemConverterComparisonPersonalCheckingProduct(IConfiguration config, FieldConverters fieldConverters)
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
//                    { "Name", page.DocumentName },
//                    { "Ctas", _fieldConverters.ConvertCtas(page.GetValue<string>("Ctas", "")) },
//                    { "Benefits", page.GetValue<string>("Benefits", "") },
//                    { "MaintenanceFee", page.GetValue<string>("MaintenanceFee", "") },
//                    { "MinimumBalanceFee", page.GetValue<string>("MinimumBalanceFee", "") },
//                    { "TransactionWithdrawalFee", page.GetValue<string>("TransactionWithdrawalFee", "") },
//                    { "InterestBearing", page.GetValue<string>("InterestBearing", "") },
//                    { "UnlimitedCheckWriting", page.GetValue<string>("UnlimitedCheckWriting", "") },
//                    { "NonBancFirstAtmFee", page.GetValue<string>("NonBancFirstAtmFee", "") },
//                    { "PaperStatementFee", page.GetValue<string>("PaperStatementFee", "") },
//                    { "estatements", page.GetValue<string>("estatements", "") },
//                }
//            };

//            return newContentItem;
//        }
//    }
//}
