using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ZAGK13Export.Models;
using System.Text.Json;
using System.ComponentModel;
using ZAGK13Export.Services;
using Microsoft.Extensions.Configuration;

namespace ZAGK13Export.Converters.Widgets
{
    class SectionConverterAccordion : ISectionConverter
    {
        public string Type => "custom.PartialAccordion";
        public string TargetType => "Custom.Components.Sections.Accordion";
        public bool IsDefault => false;
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;
        private readonly WidgetService _widgetService;
        private readonly Dictionary<string, IWidgetConverter> _widgetConverters;

        public SectionConverterAccordion(IConfiguration config, FieldConverters fieldConverters, WidgetService widgetService, IEnumerable<IWidgetConverter> widgetConverters)
        {
            _config = config;
            _fieldConverters = fieldConverters;
            _widgetService = widgetService;
            _widgetConverters = widgetConverters.ToDictionary(c => c.Type, c => c);
        }

        public Section Convert(TreeNode page)
        {
            var sectionGuid = Guid.NewGuid().ToString();
            var newSection = new Section
            {
                identifier = Guid.NewGuid().ToString(),
                type = TargetType,
                properties = new Dictionary<string, object>
                {
                    { "ariaLabel", page.DocumentName },
                    { "title", page.GetValue("Title", "") },
                    { "text", page.GetValue("Text", "") },
                    { "guid", sectionGuid }
                },
                zones = new List<Zone>(),
                fieldIdentifiers = new Dictionary<string, object>
                {
                    { "ariaLabel", Guid.NewGuid().ToString() },
                    { "title", Guid.NewGuid().ToString() },
                    { "text", Guid.NewGuid().ToString() },
                    { "panels", Guid.NewGuid().ToString() },
                    { "guid", Guid.NewGuid().ToString() }
                }
            };

            IEnumerable<TreeNode> panels = new MultiDocumentQuery()
                                .Path(page.NodeAliasPath, PathTypeEnum.Children)
                                .OnSite(_config.GetValue<string>("SourceSite"))
                                .Types("custom.PartialAccordionPanel")
                                .Culture(_config.GetValue<string>("Culture"))
                                .WithCoupledColumns()
                                .NestingLevel(1)
                                .Published().OrderBy(n => n.NodeOrder);

            if (panels.Any())
            {
                var zones = new List<Zone>();
                var zagJsonTable = new ZAGJsonTable();

                foreach (var panel in panels)
                {
                    var panelGuid = Guid.NewGuid().ToString();
                    var panelText = panel.GetValue<string>("Text", "");
                    List<Widget> widgets = new List<Widget>();

                    if(panelText != "")
                    {
                        var newRichTextWidget = new Widget
                        {
                            identifier = Guid.NewGuid().ToString(),
                            type = "Custom.Components.Widgets.RichText",
                            variants = new List<Variant>
                            {
                                { new Variant
                                    {
                                        identifier = Guid.NewGuid().ToString(),
                                        properties = new Dictionary<string, object>
                                        {
                                            { "guid", Guid.NewGuid().ToString() },
                                            { "richText", panelText }
                                        },
                                        fieldIdentifiers = new Dictionary<string, object>
                                        {
                                            { "richText", Guid.NewGuid().ToString() },
                                            { "guid", Guid.NewGuid().ToString() }
                                        }
                                    }
                                }
                            }
                        };
                        widgets.Add(newRichTextWidget);
                    }

                    widgets.AddRange(_widgetService.ConvertWidgets(panel).Result);

                    zones.Add(new Zone
                    {
                        identifier = Guid.NewGuid().ToString(),
                        name = "Section_" + sectionGuid + "_Accordion_" + panelGuid,
                        widgets = widgets
                    });

                    zagJsonTable.rows.Add(new ZAGJsonTableRows
                    {
                        guid = panelGuid,
                        type = "Unset",
                        fields = new List<ZAGJsonTableField>
                        {
                            { new ZAGJsonTableField { 
                                name = "Heading",
                                slug = "heading",
                                type = "Text",
                                value = panel.GetValue<string>("Title", ""),
                                options = [],
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }}
                        }
                    });
                }

                newSection.zones = zones;
                newSection.properties.Add("panels", JsonSerializer.Serialize(zagJsonTable.rows));
            }

            return newSection;
        }
    }
}
