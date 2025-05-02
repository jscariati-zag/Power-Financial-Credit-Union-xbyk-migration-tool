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
            await webPageManager.DestroyChannelWebPages();
        }

        public async Task AddPages()
        {
            await AddPages(xbyKImport.Pages, 0);
        }

        public async Task AddPages(List<Page> pages, int parentWebPageItemID)
        {
            // add pages in reverse order because Kentico adds new pages to the top of the list
            foreach (var page in pages.OrderByDescending(p => p.Order))
            {
                int newWebPageItemID = await AddPage(page, parentWebPageItemID);
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

                if (page.Published)
                {
                    await webPageManager.TryPublish(webPageItemID, page.Language);
                }

                var newWebPageItem = WebPageItemInfo.Provider.Get()
                                    .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageItemID)
                                    .FirstOrDefault();

                page.WebPageItemGUID = newWebPageItem.WebPageItemGUID;
                page.ContentItemGUID = ContentItemInfo.Provider.Get().FirstOrDefault(c => c.ContentItemID == newWebPageItem.WebPageItemContentItemID).ContentItemGUID;

            } else if(page.Type == "Folder")
            {
                var createFolderParameters = new CreateFolderParameters(page.DisplayName,
                                                        page.Language);

                createFolderParameters.ParentWebPageItemID = parentWebPageItemID;

                webPageItemID = await webPageManager.CreateFolder(createFolderParameters);
            }

            return webPageItemID;
        }

        public async Task UpdatePageReferences()
        {
            await UpdatePageReferences(xbyKImport.Pages, 0);
        }

        public async Task UpdatePageReferences(List<Page> pages, int parentWebPageItemID)
        {
            // add pages in reverse order because Kentico adds new pages to the top of the list
            foreach (var page in pages.OrderByDescending(p => p.Order))
            {
                await UpdatePageReference(page);
                if (page.Children != null && page.Children.Any())
                {
                    await UpdatePageReferences(page.Children, page.WebPageItemID);
                }
            }
        }

        public async Task UpdatePageReference(Page page)
        {
            if(page.Type != "Page") { return; }

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

            if (page.Published)
            {
                await webPageManager.TryPublish(page.WebPageItemID, page.Language);
            }
        }
    }
}
