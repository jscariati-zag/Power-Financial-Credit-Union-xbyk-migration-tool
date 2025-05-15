using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters.Widgets
{
    class WidgetConverterComparisonSbaLoan : IWidgetConverter
    {
        public string Type => "custom.PartialComparisonSbaLoan";
        public string TargetType => "Custom.Components.Widgets.ComparisonSbaLoan";
        private readonly FieldConverters _fieldConverters;

        public WidgetConverterComparisonSbaLoan(FieldConverters fieldConverters)
        {
            _fieldConverters = fieldConverters;
        }

        public Widget Convert(TreeNode page)
        {
            var products = page.Children
                                    .Where(c => c.ClassName == "custom.PartialComparisonSbaLoanProduct")
                                    .OrderBy(c => c.NodeOrder)
                                    .Select(r => new ContentReference
                                    {
                                        OldGuid = r.NodeGUID
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
                                { "title", page.GetValue<string>("Title", "") },
                                { "text", page.GetValue<string>("Text", "") },
                                { "products", products },
                                { "anchorText", null }
                            },
                            fieldIdentifiers = new Dictionary<string, object>
                            {
                                { "guid", Guid.NewGuid().ToString() },
                                { "key", Guid.NewGuid().ToString() },
                                { "title", Guid.NewGuid().ToString() },
                                { "text", Guid.NewGuid().ToString() },
                                { "products", Guid.NewGuid().ToString() },
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
