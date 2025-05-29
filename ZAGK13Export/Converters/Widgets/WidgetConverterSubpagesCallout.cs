using CMS.DocumentEngine;
using CMS.Relationships;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters.Widgets
{
    class WidgetConverterSubpagesCallout : IWidgetConverter
    {
        public string Type => "custom.PartialSubpagesCallout";
        public string TargetType => "Custom.Components.Widgets.SubpagesCallout";
        private readonly FieldConverters _fieldConverters;
        private readonly IConfiguration _config;

        public WidgetConverterSubpagesCallout(FieldConverters fieldConverters, IConfiguration config)
        {
            _fieldConverters = fieldConverters;
            _config = config;
        }

        public Widget Convert(TreeNode page)
        {
            TreeProvider treeProvider = new TreeProvider();
            var relationshipName = RelationshipNameInfo.Provider.Get("custom.PartialSubpagesCallout_02350bd4-9587-46e8-8647-667d172f90e5");
            var targetParent = RelationshipInfo.Provider.Get()
                .Where(r => r.RelationshipNameId == relationshipName.RelationshipNameId && r.LeftNodeId == page.NodeID)
                .OrderBy(r => r.RelationshipOrder)
                .Select(r => new ContentReference
                {
                    OldGuid = DocumentHelper.GetDocument(r.RightNodeId, _config.GetValue<string>("Culture"), treeProvider).NodeGUID
                }).ToList();

            var newWidget = new Widget
            {
                identifier = Guid.NewGuid().ToString(),
                type = TargetType,
                variants = new List<Variant>
                {
                    { new Variant
                        {
                            identifier = Guid.NewGuid().ToString(),
                            properties = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "title", page.GetValue<string>("Title", "") },
                                { "subHead", page.GetValue<string>("SubHead", "") },
                                { "body", page.GetValue<string>("Body", "") },
                                { "dropdownLabel", page.GetValue<string>("DropdownLabel", "") },
                                { "dropdownDefaultValue", page.GetValue<string>("DropdownDefaultValue", "") },
                                { "dropdownLabelPrefix", page.GetValue<string>("DropdownLabelPrefix", "") },
                                { "dropdownLabelSuffix", page.GetValue<string>("DropdownLabelSuffix", "") },
                                { "backgroundImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("BackgroundImage", "")) },
                                { "targetParent", targetParent },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "subhead", Guid.NewGuid().ToString() },
                                { "body", Guid.NewGuid().ToString() },
                                { "dropdownLabel", Guid.NewGuid().ToString() },
                                { "dropdownDefaultValue", Guid.NewGuid().ToString() },
                                { "dropdownLabelPrefix", Guid.NewGuid().ToString() },
                                { "dropdownLabelSuffix", Guid.NewGuid().ToString() },
                                { "backgroundImage", Guid.NewGuid().ToString() },
                                { "targetParent", Guid.NewGuid().ToString() },
                                { "anchorText", Guid.NewGuid().ToString() }
                            }
                        }
                    }
                }
            };

            return newWidget;
        }
    }
}
