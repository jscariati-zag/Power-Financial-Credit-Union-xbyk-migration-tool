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
        private readonly MediaService _mediaService;
        private readonly ContentItemService _contentItemService;
        private readonly PageService _pageService;
        private readonly NavigationService _navigationService;

        public ExportService(IConfiguration config, MediaService mediaService, ContentItemService contentItemService, PageService pageService, NavigationService navigationService)
        {
            _config = config;
            _mediaService = mediaService;
            _contentItemService = contentItemService;
            _pageService = pageService;
            _navigationService = navigationService;
        }

        public async Task RunAsync()
        {
            XbyKImport export = new XbyKImport();

            _mediaService.ConvertMediaLibraryFolders(export);
            _mediaService.ConvertMediaFiles(export);
            _contentItemService.AddContentHubFolders(export);
            _contentItemService.ConvertContentItems(export);
            _pageService.ConvertPages(export.Pages, null);
            _navigationService.AddNavigation(export.Pages);

            string json = JsonConvert.SerializeObject(export, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(_config.GetValue<string>("ExportFilePath"), json);

            Console.WriteLine("Export written to file.");
        }
    }
}
