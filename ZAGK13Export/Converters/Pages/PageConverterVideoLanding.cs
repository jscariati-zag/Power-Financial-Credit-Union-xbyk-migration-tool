using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using CMS.Relationships;
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
    class PageConverterVideoLanding : IPageConverter
    {
        public string Type => "custom.PageVideoLanding";
        public string TargetType => "Custom.WebPage_VideoLanding";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;
        private readonly CommonConverterService _commonConverterService;

        public PageConverterVideoLanding(IConfiguration config, FieldConverters fieldConverters, CommonConverterService commonConverterService)
        {
            _config = config;
            _fieldConverters = fieldConverters;
            _commonConverterService = commonConverterService;
        }

        public Page Convert(TreeNode page)
        {
            TreeProvider treeProvider = new TreeProvider();
            var featuredVideosRelationshipName = RelationshipNameInfo.Provider.Get("custom.PageVideoLanding_bd0500c5-b4b4-49b0-9324-ccee78b69df7");
            var featuredVideos = RelationshipInfo.Provider.Get()
                .Where(r => r.RelationshipNameId == featuredVideosRelationshipName.RelationshipNameId && r.LeftNodeId == page.NodeID)
                .OrderBy(r => r.RelationshipOrder)
                .Select(r => new RelatedPageReference
                {
                    OldGuid = DocumentHelper.GetDocument(r.RightNodeId, _config.GetValue<string>("Culture"), treeProvider).NodeGUID
                }).ToList();

            var newPage = new Page
            {
                OldGuid = page.NodeGUID,
                Type = "Page",
                DisplayName = page.DocumentName,
                ContentType = TargetType,
                WidgetConfiguration = _commonConverterService.ConvertPageWidgetsAlt(page, _config.GetValue<string>("ComponentContainerType")),
                TemplateConfiguration = new TemplateConfiguration
                {
                    identifier = "Custom.WebPage.VideoLanding"
                },
                Language = _config.GetValue<string>("TargetLanguage"),
                UrlSlug = page.GetPageUrlPath(_config.GetValue<string>("Culture")).Slug,
                Order = page.NodeOrder,
                Published = page.IsPublished,
                FormerUrls = _commonConverterService.ConvertFormerUrls(page),
                ItemData = new Dictionary<string, object>
                {
                    { "WebPage_Content_Name", page.DocumentName },
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
                    { "WebPage_MastheadTitle", page.GetValue("MastheadTitle", "") },
                    { "WebPage_MastheadText", page.GetValue("MastheadText", "") },
                    { "WebPage_MastheadCtas", _fieldConverters.ConvertCtas(page.GetValue("MastheadCtas", "")) },
                    { "WebPage_MastheadImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("MastheadImage", "")) },
                    { "WebPage_SidebarCtasTitle", page.GetValue("SidebarCtasTitle", "") },
                    { "WebPage_SubpageImage", _fieldConverters.ConvertMediaItemReference(page.GetValue("SubpageImage", "")) },
                    { "WebPage_SubpageText", page.GetValue("SubpageText", "") },
                    { "WebPage_SubpageCtas", _fieldConverters.ConvertCtas(page.GetValue("SubpageCtas", "")) },
                    { "FeaturedVideo", featuredVideos },
                    { "SidebarForm", page.GetValue<bool>("SidebarForm", false) },
                    { "SidebarFormTitle", page.GetValue("SidebarFormTitle", "") },
                }
            };

            return newPage;
        }
    }
}
