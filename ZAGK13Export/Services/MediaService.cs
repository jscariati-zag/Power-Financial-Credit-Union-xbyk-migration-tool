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
        private const string BlogImagesLibraryDisplayName = "Images";

        private static readonly HashSet<string> AllowedBlogImageFolderNames = new HashSet<string>
        {
            "Blog Images",
            "Blog Images - Updated",
            "Blog Images_June 2024",
            "Banners",
            "Promo Images"
        };

        private static bool IsAllowedBlogImagePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            string topLevelFolder = filePath.Split('/', '\\')[0];
            return AllowedBlogImageFolderNames.Contains(topLevelFolder);
        }

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
            var libraries = MediaLibraryInfo.Provider.Get().AsEnumerable<MediaLibraryInfo>();

            _contentHubFolderService.AddContentHubFolder("Imported Media", "Media", "root");

            foreach (var library in libraries)
            {
                //only bring over the blog image subfolders from the Images library
                if (library.LibraryDisplayName == BlogImagesLibraryDisplayName)
                {
                    _contentHubFolderService.AddContentHubFolder(library.LibraryDisplayName, "Media_" + library.LibraryFolder, "Media");

                    // Derive the folder hierarchy from the database FilePath of the files actually
                    // being exported (the same source of truth MediaConverter uses to build FolderName),
                    // rather than scanning the filesystem. This guarantees every folder referenced by an
                    // exported content item's FolderName is created, even if the physical directory is
                    // missing/renamed/moved on the machine running the export.
                    var mediaFiles = MediaFileInfo.Provider.Get()
                        .WhereEquals(nameof(MediaFileInfo.FileLibraryID), library.LibraryID)
                        .AsEnumerable();

                    foreach (var mediaFileInfo in mediaFiles)
                    {
                        if (!IsAllowedBlogImagePath(mediaFileInfo.FilePath))
                        {
                            continue;
                        }

                        string[] folderAr = mediaFileInfo.FilePath.Split('/');

                        // The last segment is the file name itself; everything before it is the folder path.
                        for (int depth = 1; depth < folderAr.Length; depth++)
                        {
                            string displayName = folderAr[depth - 1];
                            if (displayName == "__thumbnails")
                            {
                                continue;
                            }

                            string name = "Media_" + library.LibraryFolder + "_" + folderAr.Take(depth).Join("_");
                            string parentName = depth == 1
                                ? "Media_" + library.LibraryFolder
                                : "Media_" + library.LibraryFolder + "_" + folderAr.Take(depth - 1).Join("_");

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
                var libraryDisplayName = MediaLibraryInfo.Provider.Get(mediaFileInfo.FileLibraryID).LibraryDisplayName;

                //only export files under the blog image subfolders of the Images library
                if (libraryDisplayName == BlogImagesLibraryDisplayName && IsAllowedBlogImagePath(mediaFileInfo.FilePath))
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
