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

        public ContentItem? Convert(MediaFileInfo mediaFile)
        {
            var mediaLibraryInfo = MediaLibraryInfo.Provider.Get(mediaFile.FileLibraryID);
            string[] folderAr = mediaFile.FilePath.Split('/');
            var folderName = "Media_" + mediaLibraryInfo.LibraryName;
            if (folderAr.Length > 2)
            {
                folderName += "_" + folderAr.Take(folderAr.Length - 1).Join("_");
            }
            ContentItem? newContentItem = new ContentItem();

            if (_config.GetValue<string>("ImageExtensions").Split(';').Contains(mediaFile.FileExtension.TrimStart('.')))
            {
                newContentItem.OldGuid = mediaFile.FileGUID;
                newContentItem.DisplayName = mediaFile.FileName;
                newContentItem.ContentType = "Custom.Reusable_Image";
                newContentItem.Language = _config.GetValue<string>("TargetLanguage");
                newContentItem.FolderName = folderName;
                newContentItem.Published = true;
                newContentItem.ItemData = new Dictionary<string, object>
                {
                    { "Description", mediaFile.FileDescription },
                    { "Image", new Asset{
                        AssetUrl = MediaLibraryHelper.GetPermanentUrl(mediaFile),
                        FileGuid = mediaFile.FileGUID
                    } }
                };
            }
            else if(_config.GetValue<string>("DocumentExtensions").Split(';').Contains(mediaFile.FileExtension.TrimStart('.')))
            {
                newContentItem.OldGuid = mediaFile.FileGUID;
                newContentItem.DisplayName = mediaFile.FileName;
                newContentItem.ContentType = "Custom.Reusable_Document";
                newContentItem.Language = _config.GetValue<string>("TargetLanguage");
                newContentItem.FolderName = folderName;
                newContentItem.Published = true;
                newContentItem.ItemData = new Dictionary<string, object>
                {
                    { "Description", mediaFile.FileDescription },
                    { "Document", new Asset{
                        AssetUrl = MediaLibraryHelper.GetPermanentUrl(mediaFile),
                        FileGuid = mediaFile.FileGUID
                    } }
                };
            }
            else
            {
                return null;
            }

            return newContentItem;
        }
    }
}
