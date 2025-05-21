using CMS.DocumentEngine;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;

namespace ZAGK13Export.Converters.Navigation
{
    class SearchNavigation : INavigation
    {
        public string DisplayName => "Search Nav Items";

        private readonly IConfiguration _config;

        public SearchNavigation(IConfiguration config)
        {
            _config = config;
        }

        public List<Page> Convert()
        {
            return ConvertNavLevel("/Settings/Nav-Search", 1, 3);
        }

        public List<Page> ConvertNavLevel(string path, int level, int maxLevel)
        {
            if (level > maxLevel)
            {
                return new List<Page>();
            }

            List<Page> Pages = new List<Page>();

            var treeNodes = DocumentHelper.GetDocuments()
                    .Types("custom.Link")
                    .Path(path, PathTypeEnum.Children)
                    .NestingLevel(1)
                    .OrderBy("NodeOrder")
                    .PublishedVersion()
                    .Published()
                    .ToList();

            foreach (var node in treeNodes)
            {
                var newPage = new Page
                {
                    Type = "Page",
                    DisplayName = node.DocumentName,
                    ContentType = "Custom.Page_NavItem",
                    Language = _config.GetValue<string>("TargetLanguage"),
                    UrlSlug = node.NodeAlias,
                    Order = node.NodeOrder,
                    Published = node.IsPublished,
                    ItemData = new Dictionary<string, object>
                    {
                        { "NavItem_Content_Label", node.GetValue<string>("Text", "") },
                        { "NavItem_Link_Url", node.GetValue<string>("Url", "").TrimStart('~')},
                        { "NavItem_Link_Target", node.GetValue<string>("Target", "")},
                        { "Icon", node.GetValue<string>("IconFa", "")},
                        { "AriaLabel", node.GetValue<string>("Aria", "")}
                    }
                };

                newPage.Children = ConvertNavLevel(node.NodeAliasPath, level + 1, maxLevel);

                Pages.Add(newPage);
            }

            return Pages;
        }
    }
}
