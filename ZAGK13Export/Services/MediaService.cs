using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using Common;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;

namespace ZAGK13Export.Services
{
    class MediaService
    {
        private readonly IConfiguration _config;
        private readonly MediaConverter _mediaConverter;
        private readonly ContentHubFolderService _contentHubFolderService;

        public MediaService(IConfiguration config, MediaConverter mediaConverter, ContentHubFolderService contentHubFolderService)
        {
            _config = config;
            _mediaConverter = mediaConverter;
            _contentHubFolderService = contentHubFolderService;
        }

        public Task<IEnumerable<MediaFileInfo>> GetMediaFiles()
        {
            var mediaFiles = MediaFileInfo.Provider.Get().AsEnumerable();

            return Task.FromResult(mediaFiles);
        }

        public void ConvertMediaLibraryFolders(XbyKImport export)
        {
            var rootPath = _config.GetValue<string>("MediaPath");
            var libraries = MediaLibraryInfo.Provider.Get().AsEnumerable<MediaLibraryInfo>();

            _contentHubFolderService.AddContentHubFolder(export, "Media", "Media", "root");

            foreach (var library in libraries)
            {
                _contentHubFolderService.AddContentHubFolder(export, library.LibraryDisplayName, "Media_" + library.LibraryName, "Media");

                string[] allFolders = Directory.GetDirectories(rootPath + library.LibraryFolder, "*", SearchOption.AllDirectories);

                foreach (var folder in allFolders)
                {
                    string relativePath = Path.GetRelativePath(rootPath, folder);
                    string[] folderAr = relativePath.Split('\\');
                    string displayName = folderAr.Last();
                    string name = "Media_" + folderAr.Join("_");
                    string parentName = "Media_" + folderAr.Take(folderAr.Length - 1).Join("_");
                    if (displayName != "__thumbnails")
                    {
                        _contentHubFolderService.AddContentHubFolder(export, displayName, name, parentName);
                    }
                }
            }
        }

        public void ConvertMediaFiles(XbyKImport export)
        {
            foreach (var mediaFileInfo in GetMediaFiles().Result)
            {
                export.ContentItems.Add(ConvertMediaFile(mediaFileInfo).Result);
            }
        }

        public Task<ContentItem> ConvertMediaFile(MediaFileInfo mediaFile)
        {
            return Task.FromResult(_mediaConverter.Convert(mediaFile));
        }
    }
}
