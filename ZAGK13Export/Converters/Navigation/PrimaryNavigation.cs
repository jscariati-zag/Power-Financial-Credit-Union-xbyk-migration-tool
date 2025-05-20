using CMS.DocumentEngine;
using CMS.Ecommerce;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters.Navigation
{
    class PrimaryNavigation : INavigation
    {
        public string DisplayName => "Primary Nav Items";

        private readonly IConfiguration _config;

        public PrimaryNavigation(IConfiguration config)
        {
            _config = config;
        }

        public List<Page> Convert()
        {
            return ConvertNavLevel("/", 1, 3);
        }

        public List<Page> ConvertNavLevel(string path, int level, int maxLevel)
        {
            if (level > maxLevel)
            {
                return new List<Page>();
            }

            List<Page> Pages = new List<Page>();

            var treeNodes = DocumentHelper.GetDocuments()
                    .Types("Custom.PageDefault", "Custom.PageGroup", "Custom.PageRedirect", "Custom.PageBlog", "Custom.PageBio", "Custom.PageLanding", "Custom.PageVideoLanding", "Custom.PageVideoDetails")
                    .Path(path, PathTypeEnum.Children)
                    .NestingLevel(1)
                    .MenuItems()
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
                    Published = node.IsPublished
                };

                switch (node.ClassName)
                {
                    case "custom.PageGroup":
                        newPage.ItemData = new Dictionary<string, object>
                        {
                            { "NavItem_Content_Label", node.DocumentName }
                        };
                        break;
                    case "custom.PageRedirect":
                        newPage.ItemData = new Dictionary<string, object>
                        {
                            { "NavItem_Content_Label", node.DocumentName },
                            { "NavItem_Link_Url", node.GetValue<string>("PageRedirectUrl", "")},
                            { "NavItem_Link_Target", node.GetValue<string>("PageRedirectTarget", "")}
                        };
                        break;
                    default:
                        newPage.ItemData = new Dictionary<string, object>
                        {
                            { "NavItem_Content_Label", node.DocumentName },
                            { "NavItem_Link_Page", new RelatedPageReference
                                {
                                    OldGuid = node.NodeGUID
                                }
                            }
                        };
                        break;
                }

                newPage.Children = ConvertNavLevel(node.NodeAliasPath, level+1, maxLevel);

                Pages.Add(newPage);
            }

            return Pages;
        }
    }
}
