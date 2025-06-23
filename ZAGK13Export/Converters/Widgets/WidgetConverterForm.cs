using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.OnlineForms;
using CMS.SiteProvider;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Models;
using ZAGK13Export.Services;

namespace ZAGK13Export.Converters.Widgets
{
    class WidgetConverterForm : IWidgetConverter
    {
        public string Type => "custom.PartialForm";
        public string TargetType => "Custom.Components.Widgets.Form";
        private readonly FieldConverters _fieldConverters;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public WidgetConverterForm(FieldConverters fieldConverters, IConfiguration config, IServiceProvider serviceProvider)
        {
            _fieldConverters = fieldConverters;
            _config = config;
            _serviceProvider = serviceProvider;
        }

        public Widget? Convert(TreeNode page)
        {
            List<string> types = _serviceProvider.GetServices<IPageConverter>().Select(converter => converter.Type).ToList();
            var ancestorPage = FindFirstPageAncestor(page, types);

            if (ancestorPage == null)
            {
                return null;
            }

            string configuration = ancestorPage.GetValue<string>("DocumentPageBuilderWidgets", "");
            if (string.IsNullOrEmpty(configuration))
            {
                return null;
            }

            K13PageBuilderWidgets? pagebuilderData = JsonConvert.DeserializeObject<K13PageBuilderWidgets>(configuration);

            if (pagebuilderData == null)
            {
                return null;
            }

            foreach (var editableArea in pagebuilderData.EditableAreas)
            {
                foreach (var section in editableArea.Sections)
                {
                    foreach (var zone in section.Zones)
                    {
                        foreach (var widget in zone.Widgets)
                        {
                            if (widget.Type == "Kentico.FormWidget")
                            {
                                var formCodeName = widget.Variants[0].Properties.GetValueOrDefault("selectedForm") as string;

                                if (formCodeName == null)
                                {
                                    return null;
                                }

                                BizFormInfo formInfo = BizFormInfo.Provider.Get(formCodeName, _config.GetValue<int>("SourceSiteID"));
                                if (formInfo == null)
                                {
                                    return null;
                                }

                                string? afterSubmitMode = !string.IsNullOrEmpty(formInfo.FormRedirectToUrl) ? "URL" : "message";
                                string? afterSubmitDisplayText = formInfo.FormDisplayText;
                                string? afterSubmitRedirectToUrl = formInfo.FormRedirectToUrl != null && formInfo.FormRedirectToUrl.StartsWith("/") ? "~" + formInfo.FormRedirectToUrl : formInfo.FormRedirectToUrl;

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
                                                    { "title", page.GetValue<string>("Title", "") },
                                                    { "text", page.GetValue<string>("Text", "") },
                                                    { "selectedForm", new List<SelectedForm>{
                                                        new SelectedForm
                                                        {
                                                            objectGuid = null,
                                                            objectCodeName = formCodeName
                                                        }
                                                    }},
                                                    { "afterSubmitMode", afterSubmitMode },
                                                    { "afterSubmitDisplayText", afterSubmitDisplayText },
                                                    { "afterSubmitRedirectToUrl", afterSubmitRedirectToUrl }
                                                },
                                                fieldIdentifiers = new Dictionary<string, object>
                                                {
                                                    { "guid", Guid.NewGuid().ToString() },
                                                    { "ariaLabel", Guid.NewGuid().ToString() },
                                                    { "title", Guid.NewGuid().ToString() },
                                                    { "text", Guid.NewGuid().ToString() },
                                                    { "selectedForm", Guid.NewGuid().ToString() },
                                                    { "afterSubmitMode", Guid.NewGuid().ToString() },
                                                    { "afterSubmitDisplayText", Guid.NewGuid().ToString() },
                                                    { "afterSubmitRedirectToUrl", Guid.NewGuid().ToString() }
                                                }
                                            }
                                        }
                                    }

                                };

                                return newWidget;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public TreeNode? FindFirstPageAncestor(TreeNode node, List<string> types)
        {
            while (node != null && node.NodeParentID > 0)
            {
                node = node.Parent;

                if (types.Any(type => string.Equals(type, node.ClassName, StringComparison.OrdinalIgnoreCase)))
                {
                    return node;
                }
            }

            return null; // No matching ancestor found
        }
    }
}
