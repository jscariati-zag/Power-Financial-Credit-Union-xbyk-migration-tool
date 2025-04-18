using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.SiteProvider;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters
{
    class MediaConverter
    {
        private readonly IConfiguration _config;

        public MediaConverter(IConfiguration config)
        {
            _config = config;
        }

        public ContentItem Convert(MediaFileInfo mediaFile)
        {
            var mediaLibraryInfo = MediaLibraryInfo.Provider.Get(mediaFile.FileLibraryID);
            string[] folderAr = mediaFile.FilePath.Split('/');
            var folderName = "Media_" + mediaLibraryInfo.LibraryName;
            if (folderAr.Length > 2)
            {
                folderName += "_" + folderAr.Take(folderAr.Length - 1).Join("_");
            }
            var newContentItem = new ContentItem
            {
                OldGuid = mediaFile.FileGUID,
                DisplayName = mediaFile.FileName,
                ContentType = "Custom.Reusable_Image",
                Language = _config.GetValue<string>("TargetLanguage"),
                FolderName = folderName,
                Published = true,
                ItemData = new Dictionary<string, object>
                {
                    { "Description", mediaFile.FileDescription },
                    { "Image", new Asset{
                        AssetPath = mediaLibraryInfo.LibraryFolder + "/" + mediaFile.FilePath,
                        FileGuid = mediaFile.FileGUID
                    } }
                }
            };

            return newContentItem;
        }
    }
}
