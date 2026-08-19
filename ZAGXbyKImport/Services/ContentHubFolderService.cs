using CMS.ContentEngine;
using CMS.Membership;
using Common;
using Microsoft.Extensions.Configuration;

namespace ZAGXbyKImport.Services
{
    class ContentHubFolderService
    {
        private readonly IConfiguration config;
        private readonly IContentFolderManager contentFolderManager;
        private readonly IContentQueryExecutor contentQueryExecutor;
        private readonly XbyKImport xbyKImport;

        public ContentHubFolderService(IConfiguration config,
                                         IContentFolderManagerFactory contentFolderManagerFactory,
                                         IContentQueryExecutor contentQueryExecutor,
                                         IUserInfoProvider userInfoProvider,
                                         XbyKImport xbyKImport)
        {
            this.config = config;
            // Gets the user responsible for management operations
            // e.g., shown as the creator in 'Created by' fields
            UserInfo user = userInfoProvider.Get(config.GetValue<string>("ImportUsername"));
            // Creates an instance of the manager class facilitating content hub folder operations
            contentFolderManager = contentFolderManagerFactory.Create(user.UserID);

            this.contentQueryExecutor = contentQueryExecutor;
            this.xbyKImport = xbyKImport;
        }

        public async Task DeleteContentHubFolders()
        {
            Console.WriteLine("Deleting content hub folders...\r");

            IEnumerable<ContentFolderInfo> folders = contentFolderManager.Get().AsEnumerable();
            int totalFolders = folders.Count();
            int i = 0;
            foreach (var folder in folders)
            {
                if (!folder.IsRootFolder() && folder.ContentFolderWorkspaceID == config.GetValue<int>("WorkspaceID"))
                {
                    await contentFolderManager.Delete(folder.ContentFolderID);
                }
                i++;
                Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
                Console.Write($"\r   {totalFolders--} folders remaining");
            }
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task AddContentHubFolders()
        {
            Console.WriteLine("Adding content hub folders...\r");
            int totalFolders = xbyKImport.ContentHubFolders.Count;
            int i = 0;
            foreach (var folder in xbyKImport.ContentHubFolders)
            {
                await AddContentHubFolder(folder);
                i++;
                Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
                Console.Write($"\r   {totalFolders--} folders remaining");
            }
            Console.Write('\r' + new string(' ', Console.WindowWidth - 1));
            Console.WriteLine($"\r   COMPLETE");
        }

        public async Task AddContentHubFolder(ContentHubFolder contentHubFolder)
        {
            ContentFolderInfo parentFolder = new ContentFolderInfo();

            if(contentHubFolder.ParentName == "root")
            {
                parentFolder = await contentFolderManager.GetRoot(workspaceName: config.GetValue<string>("WorkspaceName"));
            } else
            {
                parentFolder = await contentFolderManager.Get(await GetIDFromName(contentHubFolder.ParentName));
            }

            var existingFolder = contentFolderManager.Get()
                .AsEnumerable()
                .Where(f => f.ContentFolderParentFolderID == parentFolder.ContentFolderID
                            && f.ContentFolderDisplayName == contentHubFolder.DisplayName)
                .FirstOrDefault();

            if (existingFolder != null)
            {
                contentHubFolder.ContentFolderID = existingFolder.ContentFolderID;
                return;
            }

            CreateContentFolderParameters createFolderParams = new CreateContentFolderParameters(displayName: contentHubFolder.DisplayName);

            contentHubFolder.ContentFolderID = await contentFolderManager.Create(parentFolder.ContentFolderID, createFolderParams);
        }

        public async Task AddItemToContentHubFolder(int contentItemID, int contentFolderID)
        {
            await contentFolderManager.MoveItems(contentFolderID, new List<int> { contentItemID });
        }

        public async Task<int> GetIDFromName(string name)
        {
            var folder = xbyKImport.ContentHubFolders.Where(f => f.Name == name).FirstOrDefault();
            if(folder == null)
            {
                var altName = name.Replace("-", "");
                folder = xbyKImport.ContentHubFolders.Where(f => f.Name == altName).FirstOrDefault();
            }
            if (folder == null)
            {
                throw new InvalidOperationException($"No content hub folder with name '{name}' was found in the exported data. Check that the export and import folder-naming logic match.");
            }
            return folder.ContentFolderID;
        }
    }
}
