//using CMS.DocumentEngine;
//using CMS.DocumentEngine.Routing;
//using Common;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Org.BouncyCastle.Math.EC.ECCurve;

//namespace ZAGK13Export.Converters.Pages
//{
//    class PageConverterRedirect : IPageConverter
//    {
//        public string Type => "custom.PageRedirect";
//        public string TargetType => "Custom.WebPage_Redirect";
//        private readonly IConfiguration _config;
//        private readonly FieldConverters _fieldConverters;

//        public PageConverterRedirect(IConfiguration config, FieldConverters fieldConverters)
//        {
//            _config = config;
//            _fieldConverters = fieldConverters;
//        }

//        public Page Convert(TreeNode page)
//        {
//            var newPage = new Page
//            {
//                OldGuid = page.NodeGUID,
//                Type = "Page",
//                DisplayName = page.DocumentName,
//                ContentType = TargetType,
//                Language = _config.GetValue<string>("TargetLanguage"),
//                UrlSlug = page.GetPageUrlPath(_config.GetValue<string>("Culture")).Slug,
//                Order = page.NodeOrder,
//                Published = page.IsPublished,
//                ItemData = new Dictionary<string, object>
//                {
//                    { "WebPage_Content_Name", page.DocumentName },
//                    { "WebPage_Content_HideHeaderFDIC", page.GetBooleanValue("HideHeaderFDIC", false) },
//                    { "WebPage_Inclusions_Search", false },
//                    { "WebPage_Inclusions_SitemapHtml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
//                    { "WebPage_Inclusions_SitemapXml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
//                    { "RedirectUrl", page.GetValue("PageRedirectUrl", "") },
//                    { "RedirectTarget", page.GetValue("PageRedirectTarget", "") },
//                    { "WebPage_SubpageImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("PageRedirectImage", "")) },
//                    { "WebPage_SubpageText", page.GetValue("PageRedirectText", "") },
//                    { "WebPage_SubpageCtas", _fieldConverters.ConvertCtas(page.GetValue("PageRedirectCtas", "")) },
//                }
//            };

//            return newPage;
//        }
//    }
//}
