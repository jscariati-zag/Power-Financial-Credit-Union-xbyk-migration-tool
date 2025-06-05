using Amazon.Runtime.Internal.Transform;
using CMS.DocumentEngine;
using CMS.Relationships;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;

namespace ZAGK13Export.Converters.Widgets
{
    class WidgetConverterIcons : IWidgetConverter
    {
        public string Type => "custom.PartialIcons";
        public string TargetType => "Custom.Components.Widgets.Icons";
        private readonly FieldConverters _fieldConverters;
        private readonly IConfiguration _config;

        public WidgetConverterIcons(FieldConverters fieldConverters, IConfiguration config)
        {
            _fieldConverters = fieldConverters;
            _config = config;
        }

        public Widget Convert(TreeNode page)
        {
            TreeProvider treeProvider = new TreeProvider();
            var relationshipName = RelationshipNameInfo.Provider.Get("custom.PartialIcons_db6d3629-b9f9-4fbe-b3d8-596a3701bced");
            var icons = RelationshipInfo.Provider.Get()
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
                                { "ariaLabel", page.DocumentName },
                                { "background", page.GetValue<string>("Background", "") },
                                { "icons", icons },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "ariaLabel", Guid.NewGuid().ToString() },
                                { "background", Guid.NewGuid().ToString()},
                                { "icons", Guid.NewGuid().ToString() },
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
