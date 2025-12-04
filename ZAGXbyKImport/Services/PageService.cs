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

            // Creates an instance of the manager class facilitating page operations
            this.webPageManager = webPageManagerFactory.Create(config.GetValue<int>("WebsiteChannelID"), user.UserID);
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
                var itemData = new ContentItemData(commonFunctionsService.ConvertItemData(page.ItemData, true));

                var contentItemParameters = new ContentItemParameters(page.ContentType, itemData);
                var createPageParameters = new CreateWebPageParameters(page.DisplayName,
                                                                       page.Language,
                                                                       contentItemParameters);

                if (parentWebPageItemID != 0)
                {
                    createPageParameters.ParentWebPageItemID = parentWebPageItemID;
                }
                createPageParameters.UrlSlug = page.UrlSlug;

                string templateConfiguration = page.TemplateConfiguration != null ? JsonSerializer.Serialize(page.TemplateConfiguration) : "";

                createPageParameters.SetPageBuilderConfiguration("", templateConfiguration);

                webPageItemID = await webPageManager.Create(createPageParameters);
                page.WebPageItemID = webPageItemID;

                await webPageManager.TryPublish(webPageItemID, page.Language);

                var newWebPageItem = WebPageItemInfo.Provider.Get()
                                    .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageItemID)
                                    .FirstOrDefault();

                page.WebPageItemGUID = newWebPageItem.WebPageItemGUID;
                page.ContentItemGUID = ContentItemInfo.Provider.Get().FirstOrDefault(c => c.ContentItemID == newWebPageItem.WebPageItemContentItemID).ContentItemGUID;

                if (page.FormerUrls?.Any() ?? false)
                {
                    var contentLanguageID = ContentLanguageInfo.Provider.Get().FirstOrDefault(l => l.ContentLanguageName == page.Language).ContentLanguageID;

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
                                                        page.Language);

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
                if (page.Children != null && page.Children.Any())
                {
                    await UpdatePageReferences(page.Children, page.WebPageItemID);
                }
            }
        }

        public async Task UpdatePageReference(Page page)
        {
            if (page.Type != "Page") { return; }

            ContentItemData updatedItemData = new ContentItemData(commonFunctionsService.ConvertItemData(page.ItemData, false));
            UpdateDraftData updateDraftData = new UpdateDraftData(updatedItemData);

            string templateConfiguration = page.TemplateConfiguration != null ? JsonSerializer.Serialize(page.TemplateConfiguration) : "";
            var widgetConfigurationObj = commonFunctionsService.ConvertWidgetProperties(page.WidgetConfiguration, false);
            if (widgetConfigurationObj != null)
            {
                updateDraftData.SetPageBuilderData(JsonSerializer.Serialize(widgetConfigurationObj), templateConfiguration);
            }

            await webPageManager.TryCreateDraft(page.WebPageItemID, page.Language);
            await webPageManager.TryUpdateDraft(page.WebPageItemID,
                                        page.Language,
                                        updateDraftData);


            await webPageManager.TryPublish(page.WebPageItemID, page.Language);

            if (!page.Published)
            {
                await webPageManager.TryUnpublish(page.WebPageItemID, page.Language);
            }
        }
    }
}
