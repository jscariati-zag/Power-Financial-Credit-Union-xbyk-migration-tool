using CMS.DocumentEngine;
using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZAGK13Export.Services
{
    class ExportService
    {
        private readonly IConfiguration _config;
        private readonly XbyKImport _export;
        private readonly MediaService _mediaService;
        private readonly AttachmentsService _attachmentsService;
        private readonly ContentItemService _contentItemService;
        private readonly PageService _pageService;
        private readonly NavigationService _navigationService;

        public ExportService(IConfiguration config, XbyKImport export, MediaService mediaService, AttachmentsService attachmentsService, ContentItemService contentItemService, PageService pageService, NavigationService navigationService)
        {
            _config = config;
            _export = export;
            _mediaService = mediaService;
            _attachmentsService = attachmentsService;
            _contentItemService = contentItemService;
            _pageService = pageService;
            _navigationService = navigationService;
        }

        public async Task RunAsync()
        {
            Console.Write("Converting media library folders...");
            _mediaService.ConvertMediaLibraryFolders();
            Console.WriteLine("COMPLETE");

            Console.Write("Converting media files...");
            _mediaService.ConvertMediaFiles();
            Console.WriteLine("COMPLETE");

            Console.Write("Converting attachments...");
            _attachmentsService.ConvertAttachments();
            Console.WriteLine("COMPLETE");

            Console.Write("Adding content hub folders...");
            _contentItemService.AddContentHubFolders();
            Console.WriteLine("COMPLETE");

            Console.Write("Converting content items...");
            _contentItemService.ConvertContentItems();
            Console.WriteLine("COMPLETE");

            Console.Write("Converting pages...");
            _pageService.ConvertPages(_export.Pages, null);
            Console.WriteLine("COMPLETE");

            Console.Write("Adding navigation...");
            _navigationService.AddNavigation(_export.Pages);
            Console.WriteLine("COMPLETE");

            string json = JsonConvert.SerializeObject(_export, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(_config.GetValue<string>("ExportFilePath"), json);

            Console.WriteLine("Export written to file.");
        }
    }
}
