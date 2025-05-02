using CMS.DocumentEngine;
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
    class PageConverterVideoDetail : IPageConverter
    {
        public string Type => "custom.PageVideoDetails";
        public string TargetType => "Custom.WebPage_VideoDetail";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;
        private readonly CommonConverterService _commonConverterService;

        public PageConverterVideoDetail(IConfiguration config, FieldConverters fieldConverters, CommonConverterService commonConverterService)
        {
            _config = config;
            _fieldConverters = fieldConverters;
            _commonConverterService = commonConverterService;
        }

        public Page Convert(TreeNode page)
        {
            var newPage = new Page
            {
                OldGuid = page.NodeGUID,
                Type = "Page",
                DisplayName = page.DocumentName,
                ContentType = TargetType,
                WidgetConfiguration = _commonConverterService.ConvertPageWidgets(page, _config.GetValue<string>("ComponentContainerType"), "EditableArea_01"),
                TemplateConfiguration = new TemplateConfiguration
                {
                    identifier = "Custom.WebPage.VideoDetail"
                },
                Language = _config.GetValue<string>("TargetLanguage"),
                UrlSlug = page.NodeAlias,
                Order = page.NodeOrder,
                Published = page.IsPublished,
                ItemData = new Dictionary<string, object>
                {
                    { "WebPage_Content_Name", page.DocumentName },
                    { "WebPage_Inclusions_Search", !page.DocumentSearchExcluded },
                    { "WebPage_Inclusions_SitemapHtml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                    { "WebPage_Inclusions_SitemapXml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                    { "WebPage_Seo_MetaTitle", page.DocumentPageTitle },
                    { "WebPage_Seo_MetaDescription", page.DocumentPageDescription },
                    { "WebPage_Seo_MetaKeywords", page.DocumentPageKeyWords },
                    { "WebPage_Seo_SchemaContent", page.GetValue("PageBaseSchemaContent", "") },
                    { "WebPage_Seo_CanonicalUrl", page.GetValue("PageBaseCanonicalUrl", "") },
                    { "WebPage_Og_Title", page.GetValue("PageBaseOpenGraphTitle", "") },
                    { "WebPage_Og_Type", page.GetValue("PageBaseOpenGraphType", "") },
                    { "WebPage_Og_Description", page.GetValue("PageBaseOpenGraphDescription", "") },
                    { "WebPage_Og_Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("PageBaseOpenGraphImage", "")) },
                    { "WebPage_MastheadTitle", page.GetValue("MastheadTitle", "") },
                    { "WebPage_MastheadText", page.GetValue("MastheadText", "") },
                    { "WebPage_MastheadCtas", _fieldConverters.ConvertCtas(page.GetValue("MastheadCtas", "")) },
                    { "WebPage_MastheadImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("MastheadImage", "")) },
                    { "WebPage_SidebarCtasTitle", page.GetValue("SidebarCtasTitle", "") },
                    { "WebPage_VideoTitle", page.GetValue("VideoTitle", "") },
                    { "WebPage_VideoPublishDate", page.GetDateTimeValue("VideoPublishDate", DateTime.MinValue) },
                    { "WebPage_VideoSubhead", page.GetValue("VideoSubhead", "") },
                    { "WebPage_VideoDuration", page.GetValue("VideoDuration", "") },
                    { "WebPage_VideoLongDescription", page.GetValue("VideoLongDescription", "") },
                    { "WebPage_VideoShortDescription", page.GetValue("VideoShortDescription", "") },
                    { "WebPage_VideoThumbnail", _fieldConverters.ConvertMediaItemReference(page.GetValue("VideoThumbnail", "")) },
                    { "WebPage_VideoYoutubeUrl", page.GetValue("VideoYoutubeUrl", "") },
                    { "WebPage_VideoRoundIconImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("VideoRoundIconImage", "")) },
                    { "WebPage_VideoRoundIconAltText", page.GetValue("VideoRoundIconAltText", "") }
                }
            };

            return newPage;
        }
    }
}
