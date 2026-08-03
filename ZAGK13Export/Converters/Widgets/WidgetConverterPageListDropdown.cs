//using CMS.DocumentEngine;
//using CMS.Relationships;
//using Common;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Org.BouncyCastle.Math.EC.ECCurve;

//namespace ZAGK13Export.Converters.Widgets
//{
//    class WidgetConverterPageListDropdown : IWidgetConverter
//    {
//        public string Type => "custom.PartialPageListDropdown";
//        public string TargetType => "Custom.Components.Widgets.PageListDropdown";
//        private readonly FieldConverters _fieldConverters;
//        private readonly IConfiguration _config;

//        public WidgetConverterPageListDropdown(FieldConverters fieldConverters, IConfiguration config)
//        {
//            _fieldConverters = fieldConverters;
//            _config = config;
//        }

//        public Widget Convert(TreeNode page)
//        {
//            TreeProvider treeProvider = new TreeProvider();
//            var relationshipName = RelationshipNameInfo.Provider.Get("custom.PartialPageListDropdown_9dbe4a84-874d-4a50-98e4-e76948a3f0bd");
//            var targetParent = RelationshipInfo.Provider.Get()
//                .Where(r => r.RelationshipNameId == relationshipName.RelationshipNameId && r.LeftNodeId == page.NodeID)
//                .OrderBy(r => r.RelationshipOrder)
//                .Select(r => new PageReference
//                {
//                    OldGuid = DocumentHelper.GetDocument(r.RightNodeId, _config.GetValue<string>("Culture"), treeProvider).NodeGUID
//                }).ToList();

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
//                                { "title", page.GetValue<string>("Title", "") },
//                                { "text", page.GetValue<string>("Text", "") },
//                                { "dropdownDefaultValue", page.GetValue<string>("DropdownDefaultValue", "") },
//                                { "dropdownLabelPrefix", page.GetValue<string>("DropdownLabelPrefix", "") },
//                                { "dropdownLabelSuffix", page.GetValue<string>("DropdownLabelSuffix", "") },
//                                { "targetParent", targetParent }
//                            },
//                            fieldIdentifiers = new Dictionary<string, object>
//                            {
//                                { "guid", Guid.NewGuid().ToString() },
//                                { "ariaLabel", Guid.NewGuid().ToString() },
//                                { "title", Guid.NewGuid().ToString() },
//                                { "text", Guid.NewGuid().ToString() },
//                                { "dropdownDefaultValue", Guid.NewGuid().ToString() },
//                                { "dropdownLabelPrefix", Guid.NewGuid().ToString() },
//                                { "dropdownLabelSuffix", Guid.NewGuid().ToString() },
//                                { "targetParent", Guid.NewGuid().ToString() }
//                            }
//                        }
//                    }
//                }
//            };

//            return newWidget;
//        }
//    }
//}
