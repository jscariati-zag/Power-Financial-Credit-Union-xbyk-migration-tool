using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URLRedirection;

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
        private readonly Dictionary<string, string> _languageNameCache = new Dictionary<string, string>();

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

            ContentQueryExecutionOptions contentQueryExecutionOptions = new ContentQueryExecutionOptions
            {
                ForPreview = true,
                IncludeSecuredItems = true
            };
            // Executes the query specified in 'builder' and binds it using the logic in 'ModelBinder'.
            // The second argument of the 'GetResult' method is a delegate function used to specify the mapping behavior.
            IEnumerable<ContentItemDto> result =
                    await contentQueryExecutor
                            .GetResult(builder, ModelBinder, contentQueryExecutionOptions);

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
            Console.WriteLine("Deleting content items...\r");

            var items = await GetAll();
            int totalItems = items.Count();

            int i = 0;
            foreach (var item in items)
            {
                await contentItemManager.Delete(item.SystemFields.ContentItemID, await GetLanguageName(item.SystemFields.ContentItemCommonDataContentLanguageID));
                i++;
                Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
                Console.Write($"\r   {totalItems--} items remaining");
            }

            // delete any redirects not created by Kentico's migration toolkit
            var where = new WhereCondition().WhereNotEquals("RedirectionMigrated", 1);
            RedirectionTableInfo.Provider.BulkDelete(where);
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task<string> GetLanguageName(int languageId)
        {
            ContentLanguageInfoProvider contentLanguageInfoProvider = new ContentLanguageInfoProvider();
            return contentLanguageInfoProvider.Get(languageId).ContentLanguageName;
        }

        public async Task AddContentItems()
        {
            Console.WriteLine("Adding content items...\r");
            int totalItems = xbyKImport.ContentItems.Count;
            int i = 0;
            foreach (var contentItem in xbyKImport.ContentItems)
            {
                await AddContentItem(contentItem);
                i++;
                Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
                Console.Write($"\r   {totalItems--} items remaining");
            }
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task AddContentItem(ContentItem contentItem)
        {
            int contentItemID;
            //var mediaPath = config.GetValue<string>("MediaPath");

            string resolvedLanguage = ResolveLanguageName(contentItem.Language);

            CreateContentItemParameters createParams = new CreateContentItemParameters(
                                                                contentItem.ContentType,
                                                                null,
                                                                contentItem.DisplayName,
                                                                resolvedLanguage,
                                                                config.GetValue<string>("WorkspaceName"));            

            ContentItemData itemData = new ContentItemData(commonFunctionsService.ConvertItemData(contentItem.ItemData, true, contentItem));

            // Creates the content item in the database
            contentItemID = await contentItemManager.Create(createParams, itemData);
            await contentItemManager.TryPublish(contentItemID, resolvedLanguage);

            contentItem.ContentItemID = contentItemID;

            if (!string.IsNullOrEmpty(contentItem.FolderName))
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
            Console.WriteLine("Updating content item references...\r");
            int totalItems = xbyKImport.ContentItems.Count;
            int i = 0;
            foreach (var contentItem in xbyKImport.ContentItems)
            {
                await UpdateContentItemReference(contentItem);
                i++;
                Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
                Console.Write($"\r   {totalItems--} items remaining");
            }
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task UpdateContentItemReference(ContentItem contentItem)
        {
            ContentItemData updatedItemData  = new ContentItemData(commonFunctionsService.ConvertItemData(contentItem.ItemData, false, contentItem));

            string resolvedLanguage = ResolveLanguageName(contentItem.Language);

            await contentItemManager.TryCreateDraft(contentItem.ContentItemID, resolvedLanguage); //fails: The content item with ID 0 does not exists.'
            await contentItemManager.TryUpdateDraft(contentItem.ContentItemID,
                                        resolvedLanguage,
                                        updatedItemData);

            await contentItemManager.TryPublish(contentItem.ContentItemID, resolvedLanguage);

            if (!contentItem.Published)
            {
                await contentItemManager.TryUnpublish(contentItem.ContentItemID, resolvedLanguage);
            }
        }
    }
}
