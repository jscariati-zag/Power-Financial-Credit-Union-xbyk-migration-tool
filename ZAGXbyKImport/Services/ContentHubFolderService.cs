using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
            IEnumerable<ContentFolderInfo> folders = contentFolderManager.Get().AsEnumerable();

            foreach (var folder in folders)
            {
                if (!folder.IsRootFolder() && folder.ContentFolderWorkspaceID == config.GetValue<int>("WorkspaceID"))
                {
                    await contentFolderManager.Delete(folder.ContentFolderID);
                }
            }
        }

        public async Task AddContentHubFolders()
        {
            foreach (var folder in xbyKImport.ContentHubFolders)
            {
                await AddContentHubFolder(folder);
            }
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
            return folder.ContentFolderID;
        }
    }
}
