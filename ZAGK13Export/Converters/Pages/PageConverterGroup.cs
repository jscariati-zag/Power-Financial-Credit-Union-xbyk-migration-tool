using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Services;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters.Pages
{
    class PageConverterGroup : IPageConverter
    {
        public string Type => "custom.PageGroup";
        public string TargetType => "Custom.Page_Group";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public PageConverterGroup(IConfiguration config, FieldConverters fieldConverters)
        {
            _config = config;
            _fieldConverters = fieldConverters;
        }

        public Page Convert(TreeNode page)
        {
            var newPage = new Page
            {
                OldGuid = page.NodeGUID,
                Type = "Page",
                DisplayName = page.DocumentName,
                ContentType = TargetType,
                Language = _config.GetValue<string>("TargetLanguage"),
                UrlSlug = page.GetPageUrlPath(_config.GetValue<string>("Culture")).Slug,
                Order = page.NodeOrder,
                Published = page.IsPublished,
                ItemData = new Dictionary<string, object>
                {
                    { "WebPage_Content_Name", page.DocumentName },
                    { "WebPage_Content_HideHeaderFDIC", page.GetBooleanValue("HideHeaderFDIC", false) },
                    { "WebPage_Inclusions_Search", false },
                    { "WebPage_Inclusions_SitemapHtml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                    { "WebPage_Inclusions_SitemapXml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                }
            };

            return newPage;
        }
    }
}
