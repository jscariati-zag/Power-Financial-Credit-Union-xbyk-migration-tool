using CMS.ContentEngine;
using CMS.Membership;
using CMS.Websites.Routing;
using CMS.Websites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMS.Websites.PageBuilder.Internal;
using Common;
using System.Text.Json;
using CMS.Websites.Internal;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;
using CMS.ContentEngine.Internal;
using Azure;
using CMS.Core.Internal;
using CMS.Core;
using Microsoft.Data.SqlClient;

namespace ZAGXbyKImport.Services
{
    class PageService
    {
        // Initializes all services and provider classes used within
        // the API examples on this page using dependency injection
        private readonly IConfiguration config;
        private readonly CommonFunctionsService commonFunctionsService;
        private readonly IWebPageManager webPageManager;
        private readonly IContentQueryExecutor contentQueryExecutor;
        private readonly IContentQueryResultMapper contentQueryResultMapper;
        private readonly XbyKImport xbyKImport;

        private int _remainingPages;
        private readonly Dictionary<string, string> _languageNameCache = new();

        public PageService(IConfiguration config,
                            CommonFunctionsService commonFunctionsService,
                            IContentQueryExecutor contentQueryExecutor,
                            IWebPageManagerFactory webPageManagerFactory,
                            IUserInfoProvider userInfoProvider,
                            IContentQueryResultMapper contentQueryResultMapper,
                            XbyKImport xbyKImport)
        {
            this.config = config;
            this.commonFunctionsService = commonFunctionsService;
            // Gets the user responsible for management operations.
            // The user is used only for auditing purposes,
            // e.g., to be shown as the creator in 'Created by' fields.
            // The user's permissions are not checked for any of the operations.
            UserInfo user = userInfoProvider.Get(config.GetValue<string>("ImportUsername"));
            if (user == null)
            {
                throw new InvalidOperationException($"User '{config.GetValue<string>("ImportUsername")}' not found. Check your configuration and user database.");
            }

            // Creates an instance of the manager class facilitating page operations
            this.webPageManager = webPageManagerFactory.Create(config.GetValue<int>("WebsiteChannelID"), user.UserID);
            if (this.webPageManager == null)
            {
                throw new InvalidOperationException($"WebPageManager could not be created for channel ID '{config.GetValue<int>("WebsiteChannelID")}' and user ID '{user.UserID}'. Check your configuration and channel setup.");
            }

            this.contentQueryExecutor = contentQueryExecutor;
            this.contentQueryResultMapper = contentQueryResultMapper;
            this.xbyKImport = xbyKImport;
        }

        public async Task DeletePages()
        {
            Console.WriteLine("Deleting pages...\r");
            await webPageManager.DestroyChannelWebPages();
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task AddPages()
        {
            Console.WriteLine("Adding pages...\r");
            _remainingPages = CountPages(xbyKImport.Pages);
            await AddPages(xbyKImport.Pages, 0);
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task AddPages(List<Page> pages, int parentWebPageItemID)
        {
            // apply sequential order to address cases where multiple pages had the same NodeOrder
            pages = pages.OrderBy(p => p.Order).Select((page, index) =>
            {
                page.Order = index;
                return page;
            }).ToList();

            // add pages in reverse order because Kentico adds new pages to the top of the list
            foreach (var page in pages.OrderByDescending(p => p.Order))
            {
                int newWebPageItemID = await AddPage(page, parentWebPageItemID);
                Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
                Console.Write($"\r   {_remainingPages--} pages remaining");
                if (page.Children != null && page.Children.Any())
                {
                    await AddPages(page.Children, newWebPageItemID);
                }
            }
        }

        public async Task<int> AddPage(Page page, int parentWebPageItemID)
        {
            int webPageItemID = 0;

            if (page.Type == "Page")
            {
                //Include only blog pages in import. Acknowledgement of other page types in export is required to traverse the site tree
                if (page.ContentType != "Custom.WebPage_BlogPost" && page.ContentType != "Custom.WebPage_Blog")
                {
                    return 0;
                }

                var itemData = new ContentItemData(commonFunctionsService.ConvertItemData(page.ItemData, true));

                var contentItemParameters = new ContentItemParameters(page.ContentType, itemData);

                var createPageParameters = new CreateWebPageParameters( page.DisplayName,
                                                                        ResolveLanguageName(page.Language),
                                                                        contentItemParameters);
                if (createPageParameters.Name == null)
                {
                }
                if (parentWebPageItemID != 0)
                {
                    createPageParameters.ParentWebPageItemID = parentWebPageItemID;
                }
                
                createPageParameters.UrlSlug = page.UrlSlug;

                string templateConfiguration = page.TemplateConfiguration != null ? JsonSerializer.Serialize(page.TemplateConfiguration) : "";

                createPageParameters.SetPageBuilderConfiguration("", templateConfiguration);

                if (createPageParameters == null) throw new InvalidOperationException("createPageParameters is null");
                if (createPageParameters.ContentItemParameters == null) throw new InvalidOperationException("ContentItemParameters is null");
                if (createPageParameters.ContentItemParameters.ContentItemData == null) throw new InvalidOperationException("ItemData is null");
                if (string.IsNullOrEmpty(createPageParameters.DisplayName)) throw new InvalidOperationException("DisplayName is null or empty");
                if (string.IsNullOrEmpty(createPageParameters.LanguageName)) throw new InvalidOperationException("Language is null or empty");


                //Console.WriteLine($"createPageParameters: DisplayName={createPageParameters.DisplayName}, Language={createPageParameters}, ContentType={createPageParameters.ContentItemParameters?.ContentType}, ItemDataNull={createPageParameters.ContentItemParameters?.ItemData == null}");
                ////output json information
                //Console.WriteLine(JsonSerializer.Serialize(createPageParameters));
                //Console.WriteLine(JsonSerializer.Serialize(createPageParameters.ContentItemParameters));
                //Console.WriteLine(JsonSerializer.Serialize(createPageParameters.ContentItemParameters.ContentItemData));

                //var rawItemData = commonFunctionsService.ConvertItemData(page.ItemData, true);
                ////output itemdata json information, obscured in previous output
                //Console.WriteLine("Raw ItemData: " + JsonSerializer.Serialize(page.ItemData));
                //Console.WriteLine("Converted ItemData: " + JsonSerializer.Serialize(rawItemData));

                // Do we have access to a channel info provider?
                var channelInfo = ChannelInfo.Provider.Get().WhereEquals("ChannelID", config.GetValue<int>("WebsiteChannelID")).FirstOrDefault();
                if (channelInfo == null)
                {
                    throw new InvalidOperationException("ChannelInfo not found for WebsiteChannelID.");
                }
                ////output channel info
                //Console.WriteLine($"ChannelInfo: ID={channelInfo.ChannelID}, Name={channelInfo.ChannelName}, Type={channelInfo.ChannelType}, Size={channelInfo.ChannelSize}");

                ////output connection string
                //Console.WriteLine("CMSConnectionString: " + config.GetConnectionString("CMSConnectionString"));

                //// generate minimal testing data
                    //var minimalItemData = new Dictionary<string, object>
                    //{
                    //    { "WebPage_Content_Name", "Test Page" },
                    //    { "WebPage_Alias", "test-page" }
                    //};
                    //var testitemData = new ContentItemData(minimalItemData);
                    //var testcreatePageParameters = new CreateWebPageParameters("test-page", "Test Page", "en", testcontentItemParameters);

                    //try
                    //{
                    //    if(webPageManager == null)
                    //    {
                    //        Console.WriteLine("NULL webPageManager");
                    //    }
                    //    var testId = await webPageManager.Create(createPageParameters);
                    //    Console.WriteLine("Page created with ID: " + testId);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine( createPageParameters );
                    //    Console.WriteLine("Exception: " + ex.Message);
                    //    Console.WriteLine("StackTrace: " + ex.StackTrace);
                    //    if (ex.InnerException != null)
                    //        Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                    //    throw;
                    //}
                
                if (page.ContentType == "Custom.WebPage_BlogPost")
                {
                    Console.WriteLine(createPageParameters.Name);
                }

                try
                {
                    webPageItemID = await webPageManager.Create(createPageParameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nException during page creation: " + ex.Message);
                    Console.WriteLine("\nStackTrace: " + ex.StackTrace);
                    if (ex.InnerException != null)
                        Console.WriteLine("\nInner exception: " + ex.InnerException.Message);
                    throw;
                }

                page.WebPageItemID = webPageItemID;

                await webPageManager.TryPublish(webPageItemID, ResolveLanguageName(page.Language));

                var newWebPageItem = WebPageItemInfo.Provider.Get()
                                    .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageItemID)
                                    .FirstOrDefault();

                page.WebPageItemGUID = newWebPageItem.WebPageItemGUID;
                page.ContentItemGUID = ContentItemInfo.Provider.Get().FirstOrDefault(c => c.ContentItemID == newWebPageItem.WebPageItemContentItemID).ContentItemGUID;

                if (page.FormerUrls?.Any() ?? false)
                {
                    var contentLanguageID = ContentLanguageInfo.Provider.Get().FirstOrDefault(l => l.ContentLanguageName == ResolveLanguageName(page.Language)).ContentLanguageID;

                    foreach(var formerUrl in page.FormerUrls)
                    {
                        string patchedUrl = formerUrl.TrimStart(['~']).TrimStart(['/']);
                        string urlHash = HashPath(patchedUrl);
                        var webPageFormerUrlPathInfo = new WebPageFormerUrlPathInfo
                        {
                            WebPageFormerUrlPath = patchedUrl,
                            WebPageFormerUrlPathHash = urlHash,
                            WebPageFormerUrlPathWebPageItemID = newWebPageItem.WebPageItemID,
                            WebPageFormerUrlPathWebsiteChannelID = newWebPageItem.WebPageItemWebsiteChannelID,
                            WebPageFormerUrlPathContentLanguageID = contentLanguageID,
                            WebPageFormerUrlPathLastModified = Service.Resolve<IDateTimeNowService>().GetDateTimeNow()
                        };
                        WebPageFormerUrlPathInfo.Provider.Set(webPageFormerUrlPathInfo);
                    }
                }

            } else if(page.Type == "Folder")
            {
                var createFolderParameters = new CreateFolderParameters(page.DisplayName,
                                                        ResolveLanguageName(page.Language));

                createFolderParameters.ParentWebPageItemID = parentWebPageItemID;

                webPageItemID = await webPageManager.CreateFolder(createFolderParameters);
            }

            return webPageItemID;
        }

        public int CountPages(IEnumerable<Page> pages)
        {
            int count = 0;

            foreach (var page in pages)
            {
                count++; // count this page

                if (page.Children != null && page.Children.Any())
                    count += CountPages(page.Children);
            }

            return count;
        }


        public string HashPath(string path)
        {
            using var conn = new SqlConnection(config.GetConnectionString("CMSConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', LOWER(@path)), 2)";
            cmd.Parameters.AddWithValue("path", path);
            if (cmd.ExecuteScalar() is string s)
            {
                return s;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task UpdatePageReferences()
        {
            Console.WriteLine("Updating page widgets and references...\r");
            _remainingPages = CountPages(xbyKImport.Pages);
            await UpdatePageReferences(xbyKImport.Pages, 0);
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task UpdatePageReferences(List<Page> pages, int parentWebPageItemID)
        {
            // add pages in reverse order because Kentico adds new pages to the top of the list
            foreach (var page in pages.OrderByDescending(p => p.Order))
            {
                await UpdatePageReference(page);
                _remainingPages = CountPages(xbyKImport.Pages);
                Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
                Console.Write($"\r   {_remainingPages--} pages remaining");
                Console.Write('\r' + page.DisplayName);
                if (page.Children != null && page.Children.Any())
                {
                    await UpdatePageReferences(page.Children, page.WebPageItemID);
                }
            }
        }

        public async Task UpdatePageReference(Page page)
        {
            if (page.Type != "Page") { return; }
            //if (page.ContentType == "Custom.Page_Section") { return; }
            //if (page.ContentType == "Custom.Page_MegaMenuHeading") { return; }
            //if (page.ContentType == "Custom.WebPage_Blog") { return; }

            if (page.ContentType != "Custom.WebPage_BlogPost") { return; }

            ContentItemData updatedItemData = new ContentItemData(commonFunctionsService.ConvertItemData(page.ItemData, false));
            UpdateDraftData updateDraftData = new UpdateDraftData(updatedItemData);

            string templateConfiguration = page.TemplateConfiguration != null ? JsonSerializer.Serialize(page.TemplateConfiguration) : "";
            var widgetConfigurationObj = commonFunctionsService.ConvertWidgetProperties(page.WidgetConfiguration, false);
            if (widgetConfigurationObj != null)
            {
                updateDraftData.SetPageBuilderData(JsonSerializer.Serialize(widgetConfigurationObj), templateConfiguration);
            }

            string resolvedLanguage = ResolveLanguageName(page.Language);

            await webPageManager.TryCreateDraft(page.WebPageItemID, resolvedLanguage);
            await webPageManager.TryUpdateDraft(page.WebPageItemID,
                                        resolvedLanguage,
                                        updateDraftData);


            await webPageManager.TryPublish(page.WebPageItemID, resolvedLanguage);

            if (!page.Published)
            {
                await webPageManager.TryUnpublish(page.WebPageItemID, resolvedLanguage);
            }
        }

        // Resolves the language code name to use in the target Xperience by Kentico instance.
        // The source site's language/culture code (e.g. "en-US") may not match any content
        // language configured in the target instance, so the target language is instead driven
        // by the "TargetLanguage" configuration setting.
        private string ResolveLanguageName(string sourceLanguage)
        {
            if (_languageNameCache.TryGetValue(sourceLanguage ?? string.Empty, out var cachedName))
            {
                return cachedName;
            }

            string targetLanguage = config.GetValue<string>("TargetLanguage");

            if (string.IsNullOrEmpty(targetLanguage))
            {
                throw new InvalidOperationException("The \"TargetLanguage\" configuration setting is missing or empty. Set it to a language code name that exists in the target instance (e.g. \"en\").");
            }

            var match = ContentLanguageInfo.Provider.Get()
                .FirstOrDefault(l => string.Equals(l.ContentLanguageName, targetLanguage, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                throw new InvalidOperationException($"The configured \"TargetLanguage\" ('{targetLanguage}') does not exist as a content language in the target instance.");
            }

            _languageNameCache[sourceLanguage ?? string.Empty] = match.ContentLanguageName;
            return match.ContentLanguageName;
        }
    }
}
