using CMS.DocumentEngine;
using CMS.Helpers;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Services
{
    class CommonConverterService
    {
        private readonly IConfiguration _config;
        private readonly Dictionary<string, IWidgetConverter> _widgetConverters;
        private readonly Dictionary<string, ISectionConverter> _sectionConverters;
        private readonly ISectionConverter _defaultSectionConverter;

        public CommonConverterService(IConfiguration config, IEnumerable<IWidgetConverter> widgetConverters, IEnumerable<ISectionConverter> sectionConverters)
        {
            _config = config;
            _widgetConverters = widgetConverters.ToDictionary(c => c.Type, c => c);
            _sectionConverters = sectionConverters.Where(c => !c.Type.IsNullOrEmpty()).ToDictionary(c => c.Type, c => c);
            _defaultSectionConverter = sectionConverters.Where(c => c.IsDefault).ToDictionary(c => c.Type, c => c).FirstOrDefault().Value;
        }

        public WidgetConfiguration? ConvertPageWidgets(TreeNode page, string containerType, string editableAreaIdentifier)
        {
            WidgetConfiguration? widgetConfiguration = null;

            if (page.NodeHasChildren)
            {
                IEnumerable<TreeNode> containers = new MultiDocumentQuery()
                                .Path(page.NodeAliasPath, PathTypeEnum.Children)
                                .OnSite(_config.GetValue<string>("SourceSite"))
                                .Types(containerType)
                                .Culture(_config.GetValue<string>("Culture"))
                                .WithCoupledColumns()
                                .NestingLevel(1)
                                .Published()
                                .OrderBy(n => n.NodeOrder);

                if (containers.Any())
                {
                    string sectionGuid = Guid.NewGuid().ToString();
                    List<Section> sections = new List<Section>();
                    Section currentSection = _defaultSectionConverter.Convert(null);
                    sections.Add(currentSection);

                    foreach (var container in containers)
                    {
                        if (container.NodeHasChildren)
                        {
                            IEnumerable<TreeNode> components = new MultiDocumentQuery()
                                    .Path(container.NodeAliasPath, PathTypeEnum.Children)
                                    .OnSite(_config.GetValue<string>("SourceSite"))
                                    .Types(_widgetConverters.Select(c => c.Key).Concat(_sectionConverters.Select(c => c.Key)).ToArray())
                                    .Culture(_config.GetValue<string>("Culture"))
                                    .WithCoupledColumns()
                                    .NestingLevel(1)
                                    .Published()
                                    .OrderBy(n => n.NodeOrder);

                            if (components.Any())
                            {
                                foreach (var component in components)
                                {
                                    if (_widgetConverters.TryGetValue(component.ClassName, out var widgetConverter))
                                    {
                                        if (currentSection.type != _defaultSectionConverter.TargetType)
                                        {
                                            currentSection = _defaultSectionConverter.Convert(null);
                                            sections.Add(currentSection);
                                        }
                                        var widget = widgetConverter.Convert(component);
                                        if (widget != null)
                                        {
                                            currentSection.zones[0].widgets.Add(widget);
                                        }
                                    }
                                    else if (_sectionConverters.TryGetValue(component.ClassName, out var sectionConverter))
                                    {
                                        currentSection = Task.FromResult(sectionConverter.Convert(component)).Result;
                                        sections.Add(currentSection);
                                    }
                                }
                            }
                        }
                    }

                    widgetConfiguration = new WidgetConfiguration
                    {
                        editableAreas = new List<EditableArea>
                        {
                            { new EditableArea
                                {
                                 identifier = editableAreaIdentifier,
                                 sections = sections
                                }
                            }
                        }
                    };
                }
            }

            return widgetConfiguration;
        }

        public WidgetConfiguration? ConvertPageWidgetsAlt(TreeNode page, string containerType)
        {
            WidgetConfiguration? widgetConfiguration = new WidgetConfiguration
            {
                editableAreas = new List<EditableArea>()
            };

            if (page.NodeHasChildren)
            {
                IEnumerable<TreeNode> containers = new MultiDocumentQuery()
                                .Path(page.NodeAliasPath, PathTypeEnum.Children)
                                .OnSite(_config.GetValue<string>("SourceSite"))
                                .Types(containerType)
                                .Culture(_config.GetValue<string>("Culture"))
                                .WithCoupledColumns()
                                .NestingLevel(1)
                                .Published()
                                .OrderBy(n => n.NodeOrder);

                if (containers.Any())
                {
                    string sectionGuid = Guid.NewGuid().ToString();

                    foreach (var container in containers)
                    {
                        if (container.NodeHasChildren)
                        {
                            var primaryEditableArea = new EditableArea
                            {
                                identifier = "EditableArea_Primary"
                            };

                            List<Section> sections = new List<Section>();
                            Section currentSection = _defaultSectionConverter.Convert(null);
                            sections.Add(currentSection);

                            IEnumerable<TreeNode> components = new MultiDocumentQuery()
                                    .Path(container.NodeAliasPath, PathTypeEnum.Children)
                                    .OnSite(_config.GetValue<string>("SourceSite"))
                                    .Types(_widgetConverters.Select(c => c.Key).Concat(_sectionConverters.Select(c => c.Key)).ToArray())
                                    .Culture(_config.GetValue<string>("Culture"))
                                    .WithCoupledColumns()
                                    .NestingLevel(1)
                                    .Published()
                                    .Where(n => n.GetValue<string>("Key", "Primary") == "Primary")
                                    .OrderBy(n => n.NodeOrder);

                            if (components.Any())
                            {
                                foreach (var component in components)
                                {
                                    if (_widgetConverters.TryGetValue(component.ClassName, out var widgetConverter))
                                    {
                                        if (currentSection.type != _defaultSectionConverter.TargetType)
                                        {
                                            currentSection = _defaultSectionConverter.Convert(null);
                                            sections.Add(currentSection);
                                        }
                                        var widget = widgetConverter.Convert(component);
                                        if (widget != null)
                                        {
                                            currentSection.zones[0].widgets.Add(widget);
                                        }
                                    }
                                    else if (_sectionConverters.TryGetValue(component.ClassName, out var sectionConverter))
                                    {
                                        currentSection = Task.FromResult(sectionConverter.Convert(component)).Result;
                                        sections.Add(currentSection);
                                    }
                                }
                            }

                            primaryEditableArea.sections = sections;
                            widgetConfiguration.editableAreas.Add(primaryEditableArea);

                            var columnsEditableArea = new EditableArea
                            {
                                identifier = "EditableArea_Columns"
                            };

                            List<Section> columnsSections = new List<Section>();
                            Section currentColumnsSection = _defaultSectionConverter.Convert(null);
                            columnsSections.Add(currentColumnsSection);

                            IEnumerable<TreeNode> columnsComponents = new MultiDocumentQuery()
                                    .Path(container.NodeAliasPath, PathTypeEnum.Children)
                                    .OnSite(_config.GetValue<string>("SourceSite"))
                                    .Types(_widgetConverters.Select(c => c.Key).Concat(_sectionConverters.Select(c => c.Key)).ToArray())
                                    .Culture(_config.GetValue<string>("Culture"))
                                    .WithCoupledColumns()
                                    .NestingLevel(1)
                                    .Published()
                                    .Where(n => n.GetValue<string>("Key", "Primary") == "Columns")
                                    .OrderBy(n => n.NodeOrder);

                            if (columnsComponents.Any())
                            {
                                foreach (var component in columnsComponents)
                                {
                                    if (_widgetConverters.TryGetValue(component.ClassName, out var widgetConverter))
                                    {
                                        if (currentColumnsSection.type != _defaultSectionConverter.TargetType)
                                        {
                                            currentColumnsSection = _defaultSectionConverter.Convert(null);
                                            columnsSections.Add(currentColumnsSection);
                                        }
                                        var widget = widgetConverter.Convert(component);
                                        if (widget != null)
                                        {
                                            currentColumnsSection.zones[0].widgets.Add(widget);
                                        }
                                    }
                                    else if (_sectionConverters.TryGetValue(component.ClassName, out var sectionConverter))
                                    {
                                        currentColumnsSection = Task.FromResult(sectionConverter.Convert(component)).Result;
                                        columnsSections.Add(currentColumnsSection);
                                    }
                                }
                            }

                            columnsEditableArea.sections = columnsSections;
                            widgetConfiguration.editableAreas.Add(columnsEditableArea);

                            var secondaryEditableArea = new EditableArea
                            {
                                identifier = "EditableArea_Secondary"
                            };

                            List<Section> secondarySections = new List<Section>();
                            Section currentSecondarySection = _defaultSectionConverter.Convert(null);
                            secondarySections.Add(currentSecondarySection);

                            IEnumerable<TreeNode> secondaryComponents = new MultiDocumentQuery()
                                    .Path(container.NodeAliasPath, PathTypeEnum.Children)
                                    .OnSite(_config.GetValue<string>("SourceSite"))
                                    .Types(_widgetConverters.Select(c => c.Key).Concat(_sectionConverters.Select(c => c.Key)).ToArray())
                                    .Culture(_config.GetValue<string>("Culture"))
                                    .WithCoupledColumns()
                                    .NestingLevel(1)
                                    .Published()
                                    .Where(n => n.GetValue<string>("Key", "Primary") == "Secondary")
                                    .OrderBy(n => n.NodeOrder);

                            if (secondaryComponents.Any())
                            {
                                foreach (var component in secondaryComponents)
                                {
                                    if (_widgetConverters.TryGetValue(component.ClassName, out var widgetConverter))
                                    {
                                        if (currentSecondarySection.type != _defaultSectionConverter.TargetType)
                                        {
                                            currentSecondarySection = _defaultSectionConverter.Convert(null);
                                            secondarySections.Add(currentSecondarySection);
                                        }
                                        var widget = widgetConverter.Convert(component);
                                        if (widget != null)
                                        {
                                            currentSecondarySection.zones[0].widgets.Add(widget);
                                        }
                                    }
                                    else if (_sectionConverters.TryGetValue(component.ClassName, out var sectionConverter))
                                    {
                                        currentSecondarySection = Task.FromResult(sectionConverter.Convert(component)).Result;
                                        secondarySections.Add(currentSecondarySection);
                                    }
                                }
                            }

                            secondaryEditableArea.sections = secondarySections;
                            widgetConfiguration.editableAreas.Add(secondaryEditableArea);
                        }
                    }
                }
            }

            if (!widgetConfiguration.editableAreas.Any())
            {
                return null;
            }

            return widgetConfiguration;
        }

        public List<string> ConvertFormerUrls(TreeNode page)
        {
            List<string> formerUrls = new List<string>();

            PageFormerUrlPathInfoProvider provider = new PageFormerUrlPathInfoProvider();
            var formerUrlPaths = provider.Get().WhereEquals(nameof(PageFormerUrlPathInfo.PageFormerUrlPathNodeID), page.NodeID);

            foreach(var formerUrlPath in formerUrlPaths)
            {
                formerUrls.Add(formerUrlPath.PageFormerUrlPathUrlPath);
            }

            return formerUrls;
        }
    }
}
