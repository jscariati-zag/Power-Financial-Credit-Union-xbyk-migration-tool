using Amazon.Runtime.Internal.Transform;
using CMS.DataEngine;
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
    class PageConverterBlogMonth : IPageConverter
    {
        public string Type => "custom.BlogMonth";
        public string TargetType => "Custom.Page_Group";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;
        private readonly CommonConverterService _commonConverterService;

        public PageConverterBlogMonth(IConfiguration config, FieldConverters fieldConverters, CommonConverterService commonConverterService)
        {
            _config = config;
            _fieldConverters = fieldConverters;
            _commonConverterService = commonConverterService;
        }

        public Page Convert(TreeNode page)
        {
            string urlSlug = string.Empty;
            try
            {
                var urlPathResult = page.GetPageUrlPath(_config.GetValue<string>("Culture"));
                //var urlPathResult = page.GetPageUrlPath();
                urlSlug = urlPathResult?.Slug ?? string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                // Log or handle the exception as needed
                //Console.WriteLine($"URL SLUG EMPTY: {ex} \n");
                urlSlug = page.NodeAliasPath ?? string.Empty;
            }

            var newPage = new Page
            {
                OldGuid = page.NodeGUID,
                Type = "Page",
                DisplayName = page.DocumentName,
                ContentType = TargetType,
                WidgetConfiguration = _commonConverterService.ConvertPageWidgetsAlt(page, _config.GetValue<string>("ComponentContainerType")),
                TemplateConfiguration = new TemplateConfiguration
                {
                    identifier = "Package.Blog.WebPages.BlogMonth"
                },
                Language = _config.GetValue<string>("TargetLanguage"),
                //UrlSlug = urlSlug,
                Order = page.NodeOrder,
                Published = page.IsPublished,
                FormerUrls = _commonConverterService.ConvertFormerUrls(page),
                ItemData = new Dictionary<string, object>
                {
                    { "WebPage_Content_Name", page.DocumentName },
                    { "WebPage_Alias", page.NodeAlias },
                    { "WebPage_Content_HideHeaderFDIC", page.GetBooleanValue("HideHeaderFDIC", false) },
                    { "WebPage_Inclusions_Search", !page.DocumentSearchExcluded },
                    { "WebPage_Inclusions_SitemapHtml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                    { "WebPage_Inclusions_SitemapXml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                    { "WebPage_Seo_MetaTitle", page.DocumentPageTitle },
                    { "WebPage_Seo_MetaDescription", page.DocumentPageDescription },
                    { "WebPage_Seo_MetaKeywords", page.DocumentPageKeyWords },
                    { "WebPage_Seo_SchemaContent", page.GetValue("PageBaseSchemaContent", "") },
                    { "WebPage_Og_Title", page.GetValue("PageBaseOpenGraphTitle", "") },
                    { "WebPage_Og_Type", page.GetValue("PageBaseOpenGraphType", "") },
                    { "WebPage_Og_Description", page.GetValue("PageBaseOpenGraphDescription", "") },
                    { "WebPage_Og_Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("PageBaseOpenGraphImage", "")) },
                    { "RichTextContent", page.GetValue("BlogContent", "") },
                    { "Month", page.GetValue("Month", "") },
                    { "Author", page.GetValue("Author", "") },
                    //{ "Categories", page.GetValue("Category", "") }, //this will surely need adjustment
                    { "Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("BlogImage", "")) },
                    //related posts?

                    //{ "WebPage_MastheadTitle", page.GetValue("MastheadTitle", "") },
                    //{ "WebPage_MastheadText", page.GetValue("MastheadText", "") },
                    //{ "WebPage_MastheadCtas", _fieldConverters.ConvertCtas(page.GetValue("MastheadCtas", "")) },
                    //{ "WebPage_MastheadImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("MastheadImage", "")) },
                    //{ "WebPage_SidebarCtasTitle", page.GetValue("SidebarCtasTitle", "") },
                    //{ "WebPage_SubpageImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("SubpageImage", "")) },
                    //{ "WebPage_SubpageText", page.GetValue("SubpageText", "") },
                    //{ "WebPage_SubpageCtas", _fieldConverters.ConvertCtas(page.GetValue("SubpageCtas", "")) },
                }
            };

            DateTime tempDate;
            if (DateTime.TryParse(page.GetValue("Date", ""), out tempDate))
            {
                newPage.ItemData.Add("Date", page.GetValue("Date", ""));
            }

            return newPage;
        }
    }
}
