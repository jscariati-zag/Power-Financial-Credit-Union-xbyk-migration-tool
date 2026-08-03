//using Amazon.Runtime.Internal.Transform;
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
//    class ContentItemConverterComparisonBusinessCheckingProduct : IContentItemConverter
//    {
//        public string Type => "custom.PartialComparisonBusinessCheckingProduct";
//        public string TargetType => "Custom.Reusable_ComparisonBusinessCheckingProduct";
//        public string FolderDisplayName => "Comparison Business Checking Products";
//        public string FolderName => "ComparisonBusinessCheckingProducts";
//        private readonly IConfiguration _config;
//        private readonly FieldConverters _fieldConverters;

//        public ContentItemConverterComparisonBusinessCheckingProduct(IConfiguration config, FieldConverters fieldConverters)
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
//                    { "MonthlyMaintenanceFee", page.GetValue<string>("MonthlyMaintenanceFee", "") },
//                    { "PerItemTransactionFee", page.GetValue<string>("PerItemTransactionFee", "") },
//                    { "RegulatoryAssessmentFee", page.GetValue<string>("RegulatoryAssessmentFee", "") },
//                    { "EarningsCredit", page.GetValue<string>("EarningsCredit", "") },
//                    { "CombinedAnalysisStatement", page.GetValue<string>("CombinedAnalysisStatement", "") },
//                }
//            };

//            return newContentItem;
//        }
//    }
//}
