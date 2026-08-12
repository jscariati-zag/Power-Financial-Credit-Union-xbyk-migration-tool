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
        private readonly XbyKImport _export;
        private readonly MediaConverter _mediaConverter;
        private readonly ContentHubFolderService _contentHubFolderService;

        public MediaService(IConfiguration config, XbyKImport export, MediaConverter mediaConverter, ContentHubFolderService contentHubFolderService)
        {
            _config = config;
            _export = export;
            _mediaConverter = mediaConverter;
            _contentHubFolderService = contentHubFolderService;
        }

        public Task<IEnumerable<MediaFileInfo>> GetMediaFiles()
        {
            var mediaFiles = MediaFileInfo.Provider.Get().AsEnumerable();

            return Task.FromResult(mediaFiles);
        }

        public void ConvertMediaLibraryFolders()
        {
            var rootPath = _config.GetValue<string>("MediaPath");
            var libraries = MediaLibraryInfo.Provider.Get().AsEnumerable<MediaLibraryInfo>();

            _contentHubFolderService.AddContentHubFolder("Media", "Media", "root");

            foreach (var library in libraries)
            {
                //only bring over blog images
                if(library.LibraryDisplayName == "Blog Images")
                {
                    //Console.WriteLine($"\nLibrary " + library.LibraryDisplayName);
                    _contentHubFolderService.AddContentHubFolder(library.LibraryDisplayName, "Media_" + library.LibraryName, "Media");

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
                            _contentHubFolderService.AddContentHubFolder(displayName, name, parentName);
                        }
                    }
                }
            }
        }

        public void ConvertMediaFiles()
        {
            foreach (var mediaFileInfo in GetMediaFiles().Result)
            {
                var libraryFolder = MediaLibraryInfo.Provider.Get(mediaFileInfo.FileLibraryID).LibraryFolder;
                //only export blog images
                //Console.WriteLine($"\nMedia File " + MediaLibraryInfo.Provider.Get(mediaFileInfo.FileLibraryID).LibraryFolder);
                if (libraryFolder == "Blog-Images")
                {
                    var contentItem = ConvertMediaFile(mediaFileInfo).Result;
                    if (contentItem != null)
                    {
                        _export.ContentItems.Add(contentItem);
                    }

                }
                //if (libraryFolder == "Images")
                //{
                //    Console.WriteLine(mediaFileInfo.FileName);
                //    if (mediaFileInfo.FileName == "ameris-opengraph" || mediaFileInfo.FileName == "open-graph")
                //    {
                //        var contentItem = ConvertMediaFile(mediaFileInfo).Result;
                //        if (contentItem != null)
                //        {
                //            _export.ContentItems.Add(contentItem);
                //        }
                //    }
                //}
            }
        }

        public Task<ContentItem?> ConvertMediaFile(MediaFileInfo mediaFile)
        {
            return Task.FromResult(_mediaConverter.Convert(mediaFile));
        }
    }
}
