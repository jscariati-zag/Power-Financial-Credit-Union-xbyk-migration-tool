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
    class SectionConverterColumns : ISectionConverter
    {
        public string Type => "custom.PartialColumns";
        public string TargetType => "Custom.Components.Sections.Columns";
        public bool IsDefault => false;
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;
        private readonly WidgetService _widgetService;
        private readonly Dictionary<string, IWidgetConverter> _widgetConverters;

        public SectionConverterColumns(IConfiguration config, FieldConverters fieldConverters, WidgetService widgetService, IEnumerable<IWidgetConverter> widgetConverters)
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
                    { "title", page.GetValue("Title", "") },
                    { "text", page.GetValue("Text", "") },
                    { "guid", sectionGuid },
                    { "anchorText", null }
                },
                zones = new List<Zone>(),
                fieldIdentifiers = new Dictionary<string, object>
                {
                    { "title", Guid.NewGuid().ToString() },
                    { "text", Guid.NewGuid().ToString() },
                    { "columns", Guid.NewGuid().ToString() },
                    { "guid", Guid.NewGuid().ToString() },
                    { "anchorText", Guid.NewGuid().ToString() }
                }
            };

            IEnumerable<TreeNode> columns = new MultiDocumentQuery()
                                .Path(page.NodeAliasPath, PathTypeEnum.Children)
                                .OnSite(_config.GetValue<string>("SourceSite"))
                                .Types("custom.PartialColumn")
                                .Culture(_config.GetValue<string>("Culture"))
                                .WithCoupledColumns()
                                .NestingLevel(1)
                                .Published();

            if (columns.Any())
            {
                var zones = new List<Zone>();
                var zagJsonTable = new ZAGJsonTable();

                foreach (var column in columns)
                {
                    var columnGuid = Guid.NewGuid().ToString();
                    var columnText = column.GetValue<string>("Text", "");
                    List<Widget> widgets = new List<Widget>();

                    if (columnText != "")
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
                                            { "richText", columnText },
                                            { "anchorText", null }
                                        },
                                        fieldIdentifiers = new Dictionary<string, object>
                                        {
                                            { "richText", Guid.NewGuid().ToString() },
                                            { "guid", Guid.NewGuid().ToString() },
                                            { "anchorText", Guid.NewGuid().ToString() }
                                        }
                                    }
                                }
                            }
                        };
                        widgets.Add(newRichTextWidget);
                    }

                    widgets.AddRange(_widgetService.ConvertWidgets(column).Result);

                    zones.Add(new Zone
                    {
                        identifier = Guid.NewGuid().ToString(),
                        name = "Section_" + sectionGuid + "_Column_" + columnGuid,
                        widgets = widgets
                    });

                    zagJsonTable.rows.Add(new ZAGJsonTableRows
                    {
                        guid = columnGuid,
                        type = "Unset",
                        fields = new List<ZAGJsonTableField>
                        {
                            { new ZAGJsonTableField { 
                                name = "CssClasses",
                                slug = "cssClasses",
                                type = "Dropdown",
                                value = column.GetValue<string>("Class", ""),
                                options = new List<string> {
                                    "col-md-3;25%",
                                    "col-md-4;33%",
                                    "col-md-6;50%",
                                    "col-md-8;66%",
                                    "col-md-9;75%",
                                    "col-md;Auto",
                                    "other;Other"
                                },
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }},
                            { new ZAGJsonTableField {
                                name = "CssClassesOther",
                                slug = "cssClassesOther",
                                type = "Text",
                                value = "",
                                options = new List<string>(),
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }},
                        }
                    });
                }

                newSection.zones = zones;
                newSection.properties.Add("columns", JsonSerializer.Serialize(zagJsonTable.rows));
            }

            return newSection;
        }
    }
}
