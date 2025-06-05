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
    class WidgetConverterTwoCrossPromos : IWidgetConverter
    {
        public string Type => "custom.PartialTwoCrossPromos";
        public string TargetType => "Custom.Components.Widgets.TwoCrossPromos";
        private readonly FieldConverters _fieldConverters;
        private readonly IConfiguration _config;

        public WidgetConverterTwoCrossPromos(FieldConverters fieldConverters, IConfiguration config)
        {
            _fieldConverters = fieldConverters;
            _config = config;
        }

        public Widget Convert(TreeNode page)
        {
            TreeProvider treeProvider = new TreeProvider();
            var relationshipName = RelationshipNameInfo.Provider.Get("custom.PartialTwoCrossPromos_c7b348da-23ab-41ff-84d4-a4feea5350b4");
            var promos = RelationshipInfo.Provider.Get()
                .Where(r => r.RelationshipNameId == relationshipName.RelationshipNameId && r.LeftNodeId == page.NodeID)
                .OrderBy(r => r.RelationshipOrder)
                .Select(r => new ContentReference
                {
                        OldGuid = DocumentHelper.GetDocument(r.RightNodeId, _config.GetValue<string>("Culture"), treeProvider).NodeGUID
                }).Take(2).ToList();

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
                                { "promoTitleH1", page.GetBooleanValue("PromoTitleH1", false) },
                                { "promos", promos },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "ariaLabel", Guid.NewGuid().ToString() },
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
