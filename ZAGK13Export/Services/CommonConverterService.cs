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
                                        currentSection.zones[0].widgets.Add(Task.FromResult(widgetConverter.Convert(component)).Result);
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
    }
}
