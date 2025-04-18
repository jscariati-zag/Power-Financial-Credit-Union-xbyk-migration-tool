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
            Console.Write("Deleting content items...");
            await contentItemService.DeleteContentItems();
            Console.WriteLine("COMPLETE");

            Console.Write("Deleting content hub folders...");
            await contentHubFolderService.DeleteContentHubFolders();
            Console.WriteLine("COMPLETE");

            Console.Write("Deleting pages...");
            await pageService.DeletePages();
            Console.WriteLine("COMPLETE");

            Console.Write("Adding content hub folders...");
            await contentHubFolderService.AddContentHubFolders();
            Console.WriteLine("COMPLETE");

            Console.Write("Adding content items...");
            await contentItemService.AddContentItems();
            Console.WriteLine("COMPLETE");

            Console.Write("Adding pages...");
            await pageService.AddPages();
            Console.WriteLine("COMPLETE");

            Console.Write("Updating content item references...");
            await contentItemService.UpdateContentItemReferences();
            Console.WriteLine("COMPLETE");

            Console.Write("Updating page widgets and references...");
            await pageService.UpdatePageReferences();
            Console.WriteLine("COMPLETE");

            Console.WriteLine("Import file processed.");

        }
    }
}
