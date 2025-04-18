using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGXbyKImport.Services
{
    class ContentItemService
    {
        private readonly IConfiguration config;
        private readonly CommonFunctionsService commonFunctionsService;
        private readonly IContentItemManager contentItemManager;
        private readonly IContentQueryExecutor contentQueryExecutor;
        private readonly IContentItemCodeNameProvider contentItemCodeNameProvider;
        private readonly IContentQueryResultMapper contentQueryResultMapper;
        private readonly ContentHubFolderService contentHubFolderService;
        private readonly XbyKImport xbyKImport;

        public string[] ContentTypes;

        public ContentItemService(IConfiguration config,
                                    CommonFunctionsService commonFunctionsService,
                                    IContentItemManagerFactory contentItemManagerFactory,
                                    IContentQueryExecutor contentQueryExecutor,
                                    IUserInfoProvider userInfoProvider,
                                    IContentItemCodeNameProvider contentItemCodeNameProvider,
                                    IContentQueryResultMapper contentQueryResultMapper,
                                    ContentHubFolderService contentHubFolderService,
                                    XbyKImport xbyKImport)
        {
            this.config = config;
            this.commonFunctionsService = commonFunctionsService;
            // Gets the user responsible for management operations.
            // The user is used only for auditing purposes,
            // e.g., to be shown as the creator in 'Created by' fields.
            // The user's permissions are not checked for any of the operations.
            UserInfo user = userInfoProvider.Get(config.GetValue<string>("ImportUsername"));

            // Creates an instance of the manager class facilitating content item operations
            this.contentItemManager = contentItemManagerFactory.Create(user.UserID);
            this.contentQueryExecutor = contentQueryExecutor;
            this.contentItemCodeNameProvider = contentItemCodeNameProvider;
            this.contentQueryResultMapper = contentQueryResultMapper;
            this.contentHubFolderService = contentHubFolderService;
            this.xbyKImport = xbyKImport;

            DataClassInfoProvider dataClassInfoProvider = new DataClassInfoProvider();
            ContentTypes = dataClassInfoProvider.Get().Where(c => c.ClassContentTypeType == "Reusable").ToList().Select(c => c.ClassName).ToArray();
        }

        public async Task<ContentItemDto> GetContentItem(int contentItemID)
        {
            ContentItemQueryBuilder builder =
                new ContentItemQueryBuilder()
                    .ForContentTypes(subqueryParameters =>
                    {
                        subqueryParameters.OfContentType(ContentTypes);
                    }).Parameters(subqueryParameters =>
                        subqueryParameters.TopN(1).Where(w => w.WhereEquals("ContentItemID", contentItemID)));

            // Executes the query specified in 'builder' and binds it using the logic in 'ModelBinder'.
            // The second argument of the 'GetResult' method is a delegate function used to specify the mapping behavior.
            ContentItemDto result =
                    (await contentQueryExecutor
                            .GetResult(builder, ModelBinder)).FirstOrDefault();

            return result;
        }

        public async Task<IEnumerable<ContentItemDto>> GetAll()
        {
            ContentItemQueryBuilder builder =
                new ContentItemQueryBuilder()
                    .ForContentTypes(subqueryParameters =>
                    {
                        subqueryParameters.OfContentType(ContentTypes);
                    }).InWorkspaces(config.GetValue<string>("WorkspaceName"));

            // Executes the query specified in 'builder' and binds it using the logic in 'ModelBinder'.
            // The second argument of the 'GetResult' method is a delegate function used to specify the mapping behavior.
            IEnumerable<ContentItemDto> result =
                    await contentQueryExecutor
                            .GetResult(builder, ModelBinder);

            return result;
        }

        private ContentItemDto ModelBinder(IContentQueryDataContainer container)
        {
            // Maps column data to corresponding properties of the custom model class based on matching names. For the example 'Dto' object,
            // only columns named 'Name', 'ProductRating' and 'ShortDescription' get mapped in addition to
            // 'SystemFields'. All other fields of content types are ignored.
            return contentQueryResultMapper.Map<ContentItemDto>(container);
        }

        public class ContentItemDto
        {
            // Maps Xperience-specific fields
            public ContentItemFields SystemFields { get; set; }
        }

        public async Task DeleteContentItems()
        {
            var items = await GetAll();

            foreach (var item in items)
            {
                await contentItemManager.Delete(item.SystemFields.ContentItemID, await GetLanguageName(item.SystemFields.ContentItemCommonDataContentLanguageID));
            }
        }

        public async Task<string> GetLanguageName(int languageId)
        {
            ContentLanguageInfoProvider contentLanguageInfoProvider = new ContentLanguageInfoProvider();
            return contentLanguageInfoProvider.Get(languageId).ContentLanguageName;
        }

        public async Task AddContentItems()
        {
            foreach (var contentItem in xbyKImport.ContentItems)
            {
                await AddContentItem(contentItem);
            }
        }

        public async Task AddContentItem(ContentItem contentItem)
        {
            int contentItemID;
            var mediaPath = config.GetValue<string>("MediaPath");

            CreateContentItemParameters createParams = new CreateContentItemParameters(
                                                                contentItem.ContentType,
                                                                null,
                                                                contentItem.DisplayName,
                                                                contentItem.Language,
                                                                config.GetValue<string>("WorkspaceName"));            

            ContentItemData itemData = new ContentItemData(commonFunctionsService.ConvertItemData(contentItem.ItemData, true));

            // Creates the content item in the database
            contentItemID = await contentItemManager.Create(createParams, itemData);
            if (contentItem.Published)
            {
                await contentItemManager.TryPublish(contentItemID, contentItem.Language);
            }
            contentItem.ContentItemID = contentItemID;

            if (!contentItem.FolderName.IsNullOrEmpty())
            {
                await contentHubFolderService.AddItemToContentHubFolder(contentItem.ContentItemID, await contentHubFolderService.GetIDFromName(contentItem.FolderName));
            }

            var newContentItem = ContentItemInfo.Provider.Get()
                                .WhereEquals(nameof(ContentItemInfo.ContentItemID), contentItemID)
                                .FirstOrDefault();

            contentItem.ContentItemGUID = newContentItem.ContentItemGUID;
        }

        public async Task UpdateContentItemReferences()
        {
            foreach (var contentItem in xbyKImport.ContentItems)
            {
                await UpdateContentItemReference(contentItem);
            }
        }

        public async Task UpdateContentItemReference(ContentItem contentItem)
        {
            ContentItemData updatedItemData  = new ContentItemData(commonFunctionsService.ConvertItemData(contentItem.ItemData, false));

            await contentItemManager.TryCreateDraft(contentItem.ContentItemID, contentItem.Language);
            await contentItemManager.TryUpdateDraft(contentItem.ContentItemID,
                                        contentItem.Language,
                                        updatedItemData);

            if (contentItem.Published)
            {
                await contentItemManager.TryPublish(contentItem.ContentItemID, contentItem.Language);
            }
        }
    }
}
