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
    class WidgetConverterCrossPromos : IWidgetConverter
    {
        public string Type => "custom.PartialCrossPromos";
        public string TargetType => "Custom.Components.Widgets.CrossPromos";
        private readonly FieldConverters _fieldConverters;
        private readonly IConfiguration _config;

        public WidgetConverterCrossPromos(FieldConverters fieldConverters, IConfiguration config)
        {
            _fieldConverters = fieldConverters;
            _config = config;
        }

        public Widget Convert(TreeNode page)
        {
            TreeProvider treeProvider = new TreeProvider();
            var relationshipName = RelationshipNameInfo.Provider.Get("custom.PartialCrossPromos_db90da38-a229-42b4-8126-15318985bc09");
            var promos = RelationshipInfo.Provider.Get()
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
                                { "key", page.GetValue<string>("Key", "") },
                                { "promoTitleH1", page.GetBooleanValue("PromoTitleH1", false) },
                                { "promos", promos },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "key", Guid.NewGuid().ToString() },
                                { "promoTitleH1", Guid.NewGuid().ToString()},
                                { "promos", Guid.NewGuid().ToString() },
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
