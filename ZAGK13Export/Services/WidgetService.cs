using CMS.DocumentEngine;
using CMS.Helpers;
using Common;
using DocumentFormat.OpenXml.Presentation;
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
    class WidgetService
    {
        private readonly IConfiguration _config;
        private readonly Dictionary<string, IWidgetConverter> _widgetConverters;

        public WidgetService(IConfiguration config, IEnumerable<IWidgetConverter> widgetConverters)
        {
            _config = config;
            _widgetConverters = widgetConverters.ToDictionary(c => c.Type, c => c);
        }

        public Task<List<Widget>> ConvertWidgets(TreeNode parent)
        {
            IEnumerable<TreeNode> components = new MultiDocumentQuery()
                                    .Path(parent.NodeAliasPath, PathTypeEnum.Children)
                                    .OnSite(_config.GetValue<string>("SourceSite"))
                                    .Types(_widgetConverters.Select(c => c.Key).ToArray())
                                    .Culture(_config.GetValue<string>("Culture"))
                                    .WithCoupledColumns()
                                    .NestingLevel(1)
                                    .Published()
                                    .OrderBy(n => n.NodeOrder);

            return ConvertWidgets(components);
        }

        public Task<List<Widget>> ConvertWidgets(IEnumerable<TreeNode> components)
        {
            List<Widget> widgets = new List<Widget>();

            foreach (var component in components)
            {
                var widget = ConvertWidget(component.ClassName, component).Result;
                if (widget != null)
                {
                    widgets.Add(ConvertWidget(component.ClassName, component).Result);
                }
            }

            return Task.FromResult(widgets);
        }

        public Task<Widget> ConvertWidget(string type, TreeNode page)
        {
            if (_widgetConverters.TryGetValue(type, out var converter))
            {
                return Task.FromResult(converter.Convert(page));
            }

            throw new ArgumentException($"No widget converter found for type: {type}");
        }
    }
}
