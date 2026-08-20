using Amazon.Runtime.Internal.Transform;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using CMS.Taxonomy;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZAGK13Export.Services;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters.Pages
{
    class PageConverterBlogPost : IPageConverter
    {
        public string Type => "custom.BlogDetail";
        public string TargetType => "Custom.WebPage_BlogPost";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;
        private readonly CommonConverterService _commonConverterService;

        public PageConverterBlogPost(IConfiguration config, FieldConverters fieldConverters, CommonConverterService commonConverterService)
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
                    identifier = "Custom.Web.WebPages.BlogPost"
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
                    { "WebPage_Inclusions_Search", !page.DocumentSearchExcluded },
                    { "WebPage_Inclusions_SitemapHtml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                    { "WebPage_Inclusions_SitemapXml", !page.GetBooleanValue("DocumentSitemapExcluded", false) },
                    { "WebPage_Seo_MetaTitle", page.DocumentPageTitle },
                    { "WebPage_Seo_MetaDescription", page.DocumentPageDescription },
                    { "WebPage_Seo_MetaKeywords", page.DocumentPageKeyWords },
                    { "WebPage_Seo_SchemaContent", page.GetValue("SchemaContent", "") },
                    { "WebPage_Og_Title", page.GetValue("OpenGraphTitle", "") },
                    { "WebPage_Og_Type", page.GetValue("OpenGraphType", "") },
                    { "WebPage_Og_Description", page.GetValue("OpenGraphDescription", "") },
                    { "WebPage_Og_Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("OpenGraphImage", "")) },
                    //{ "Date", page.GetValue("Date", "") },
                    //{ "Categories", page.GetValue("Category", "") }, //this will surely need adjustment
                    { "Image", _fieldConverters.ConvertMediaItemReference(page.GetValue("BannerImage", "")) },
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

            string articleContent = page.GetValue("ArticleContent", "");
            int? readingTime = ExtractReadingTime(ref articleContent);
            if (readingTime.HasValue)
            {
                newPage.ItemData.Add("ReadingTime", readingTime.Value);
            }

            string author = page.GetValue("Author", "");
            if (!string.IsNullOrWhiteSpace(author))
            {
                newPage.ItemData.Add("Authors", new List<TaxonomyTagReference>
                {
                    new TaxonomyTagReference
                    {
                        TaxonomyName = "Authors",
                        TagName = $"Authors_{GetCodeName(author)}",
                        TagTitle = author
                    }
                });
            }

            var categoryReferences = GetBlogCategoryTagReferences(page);
            if (categoryReferences.Any())
            {
                newPage.ItemData.Add("Categories", categoryReferences);
            }

            newPage.WidgetConfiguration = _commonConverterService.AddRichTextWidgetToConfiguration(
                newPage.WidgetConfiguration,
                "EditableArea_01",
                articleContent);

            return newPage;
        }

        private static readonly Regex NonAlphanumericRegex = new Regex(@"[^A-Za-z0-9]", RegexOptions.Compiled);

        private static string GetCodeName(string value)
        {
            return NonAlphanumericRegex.Replace(value, string.Empty);
        }

        // The source site organizes blog categories as children of a "Blog" parent category.
        // Only those child categories assigned to the document are migrated as "Blog" taxonomy tags.
        //
        // Categories can be assigned to a document in two different ways on this site:
        //  1. The native CMS category assignment mechanism (CMS_DocumentCategory binding table),
        //     readable via CategoryInfoProvider.GetDocumentCategories.
        //  2. A custom "Category" field on custom.BlogDetail that uses a Kentico "Uni selector"
        //     form control, which stores its selection directly as a semicolon-separated list of
        //     values in the field itself (bypassing the native binding table entirely). Depending on
        //     configuration, a Uni selector can persist CategoryID, CategoryName (code name), or
        //     CategoryDisplayName, so all three are attempted when resolving each token.
        // Both sources are checked and merged (de-duplicated) here, since some posts only use one or the other.
        private List<TaxonomyTagReference> GetBlogCategoryTagReferences(TreeNode page)
        {
            var references = new List<TaxonomyTagReference>();
            var addedCategoryIds = new HashSet<int>();
            string siteName = _config.GetValue<string>("SourceSite");

            string categoryField = page.GetValue("Category", "");
            if (!string.IsNullOrWhiteSpace(categoryField))
            {
                var categoryTokens = categoryField.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var rawToken in categoryTokens)
                {
                    string token = rawToken.Trim();
                    if (token.Length == 0)
                    {
                        continue;
                    }

                    var category = ResolveCategory(token, siteName);
                    if (category != null && addedCategoryIds.Add(category.CategoryID))
                    {
                        AddBlogCategoryTagReference(category, references);
                    }
                }
            }

            var documentCategories = CategoryInfoProvider.GetDocumentCategories($"DocumentID = {page.DocumentID}");
            foreach (CategoryInfo category in documentCategories)
            {
                if (addedCategoryIds.Add(category.CategoryID))
                {
                    AddBlogCategoryTagReference(category, references);
                }
            }

            return references;
        }

        private static CategoryInfo ResolveCategory(string token, string siteName)
        {
            // The Uni selector on this field is configured with "Return column name: CategoryDisplayName",
            // so tokens are expected to be category display names. The ID/code name checks are kept as a
            // defensive fallback in case the field ever contains a differently-configured value.
            var categoryByDisplayName = CategoryInfoProvider.GetCategories()
                .WhereEquals(nameof(CategoryInfo.CategoryDisplayName), token)
                .FirstOrDefault();
            if (categoryByDisplayName != null)
            {
                return categoryByDisplayName;
            }

            if (int.TryParse(token, out int categoryId))
            {
                var categoryById = CategoryInfoProvider.GetCategoryInfo(categoryId);
                if (categoryById != null)
                {
                    return categoryById;
                }
            }

            return CategoryInfoProvider.GetCategoryInfo(token, siteName);
        }

        private static void AddBlogCategoryTagReference(CategoryInfo category, List<TaxonomyTagReference> references)
        {
            var parentCategory = category.CategoryParentID > 0
                ? CategoryInfoProvider.GetCategoryInfo(category.CategoryParentID)
                : null;

            if (parentCategory == null || !string.Equals(parentCategory.CategoryDisplayName, "Blog", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            references.Add(new TaxonomyTagReference
            {
                TaxonomyName = "Blog",
                TagName = $"Blog_{GetCodeName(category.CategoryDisplayName)}",
                TagTitle = category.CategoryDisplayName
            });
        }

        // Detects a leading reading time estimate (e.g. "5 MIN. READ", "5 min read") at the
        // start of the article content, extracts it as an integer, and removes the text
        // (including any surrounding markup/whitespace) from the content.
        private static readonly Regex ReadingTimeRegex = new Regex(
            @"^\s*(?:<[^>]+>\s*)*(\d+)\s*MIN(?:UTE)?S?\.?\s*READ\s*(?:</[^>]+>\s*)*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static int? ExtractReadingTime(ref string articleContent)
        {
            if (string.IsNullOrEmpty(articleContent))
            {
                return null;
            }

            var match = ReadingTimeRegex.Match(articleContent);
            if (!match.Success)
            {
                return null;
            }

            if (!int.TryParse(match.Groups[1].Value, out int minutes))
            {
                return null;
            }

            articleContent = articleContent.Substring(match.Length);
            return minutes;
        }
    }
}
