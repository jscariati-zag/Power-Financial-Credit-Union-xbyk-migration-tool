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

            // The source site stores blog posts under a BlogYear > BlogMonth hierarchy, but the
            // target site combines both levels into a single "Month Year" group page. BlogYear is
            // exported as a pass-through node (see PageConverterBlogYear), so derive the year here
            // from the node's alias path (e.g. "/Blog/2018/April" -> "2018") to build the combined name.
            string[] pathSegments = (page.NodeAliasPath ?? string.Empty).Trim('/').Split('/');
            string year = pathSegments.Length >= 2 ? pathSegments[^2] : string.Empty;
            string combinedDisplayName = string.IsNullOrEmpty(year) ? page.DocumentName : $"{page.DocumentName} {year}";
            string combinedAlias = string.IsNullOrEmpty(year) ? page.NodeAlias : $"{page.NodeAlias}-{year}";

            // Order the combined "Month Year" group pages newest-to-oldest as siblings under Blog.
            // Lower Order values end up first/topmost after import (see PageService.AddPages), so
            // compute a value that decreases as the month/year gets more recent.
            int order = int.MaxValue;
            if (int.TryParse(year, out int yearNumber) &&
                DateTime.TryParseExact(page.DocumentName, "MMMM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime monthDate))
            {
                order = int.MaxValue - (yearNumber * 12 + monthDate.Month);
            }

            var newPage = new Page
            {
                OldGuid = page.NodeGUID,
                Type = "Page",
                DisplayName = combinedDisplayName,
                ContentType = TargetType,
                WidgetConfiguration = _commonConverterService.ConvertPageWidgetsAlt(page, _config.GetValue<string>("ComponentContainerType")),
                Language = _config.GetValue<string>("TargetLanguage"),
                //UrlSlug = urlSlug,
                Order = order,
                Published = page.IsPublished,
                FormerUrls = _commonConverterService.ConvertFormerUrls(page),
                ItemData = new Dictionary<string, object>
                {
                    { "WebPage_Content_Name", combinedDisplayName },
                    { "WebPage_Alias", combinedAlias },
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
