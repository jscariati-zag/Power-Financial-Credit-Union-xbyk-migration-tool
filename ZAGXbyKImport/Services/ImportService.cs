using CMS.ContentEngine;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZAGXbyKImport.Services
{
    class ImportService
    {
        private readonly PageService pageService;
        private readonly ContentItemService contentItemService;
        private readonly ContentHubFolderService contentHubFolderService;
        private readonly XbyKImport xbyKImport;

        public ImportService(PageService pageService, ContentItemService contentItemService, ContentHubFolderService contentHubFolderService, XbyKImport xbyKImport )
        {
            this.pageService = pageService;
            this.contentItemService = contentItemService;
            this.contentHubFolderService = contentHubFolderService;
            this.xbyKImport = xbyKImport;
        }
        public async Task RunAsync()
        {
            await contentItemService.DeleteContentItems();

            await contentHubFolderService.DeleteContentHubFolders();

            await pageService.DeletePages();

            await contentHubFolderService.AddContentHubFolders();

            await contentItemService.AddContentItems();
 
            await pageService.AddPages();

            await contentItemService.UpdateContentItemReferences();

            await pageService.UpdatePageReferences();

            Console.WriteLine("Import file processed.");

        }
    }
}
