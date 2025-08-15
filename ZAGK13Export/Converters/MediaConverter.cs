using CMS.Helpers;
using CMS.MediaLibrary;
using Common;
using Microsoft.Extensions.Configuration;

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
            var folderName = "Media_" + mediaLibraryInfo.LibraryFolder;
            if (folderAr.Length > 1)
            {
                folderName += "_" + folderAr.Take(folderAr.Length - 1).Join("_");
            }
            ContentItem? newContentItem = new ContentItem();

            if (_config.GetValue<string>("ImageExtensions").Split(';').Contains(mediaFile.FileExtension.TrimStart('.')))
            {
                newContentItem.OldGuid = mediaFile.FileGUID;
                newContentItem.OldDirectUrl = "/" + _config.GetValue<string>("SourceSite") + "/media/" + mediaLibraryInfo.LibraryFolder + "/" + mediaFile.FilePath;
                newContentItem.DisplayName = mediaFile.FileName.Length > 100 ? mediaFile.FileName.Substring(0, 100) : mediaFile.FileName;
                newContentItem.ContentType = "Custom.Reusable_Image";
                newContentItem.Language = _config.GetValue<string>("TargetLanguage");
                newContentItem.FolderName = folderName;
                newContentItem.Published = true;
                newContentItem.ItemData = new Dictionary<string, object>
                {
                    { "Description", mediaFile.FileDescription.Length > 200 ? mediaFile.FileDescription.Substring(0, 200) : mediaFile.FileDescription },
                    { "Asset_Image", new Asset{
                        AssetUrl = MediaLibraryHelper.GetPermanentUrl(mediaFile),
                        FileGuid = mediaFile.FileGUID
                    } }
                };
            }
            else
            {
                newContentItem.OldGuid = mediaFile.FileGUID;
                newContentItem.OldDirectUrl = "/" + _config.GetValue<string>("SourceSite") + "/media/" + mediaLibraryInfo.LibraryFolder + "/" + mediaFile.FilePath;
                newContentItem.DisplayName = mediaFile.FileName.Length > 100 ? mediaFile.FileName.Substring(0, 100) : mediaFile.FileName;
                newContentItem.ContentType = "Custom.Reusable_Document";
                newContentItem.Language = _config.GetValue<string>("TargetLanguage");
                newContentItem.FolderName = folderName;
                newContentItem.Published = true;
                newContentItem.ItemData = new Dictionary<string, object>
                {
                    { "Description", mediaFile.FileDescription.Length > 200 ? mediaFile.FileDescription.Substring(0, 200) : mediaFile.FileDescription },
                    { "Asset_Document", new Asset{
                        AssetUrl = MediaLibraryHelper.GetPermanentUrl(mediaFile),
                        FileGuid = mediaFile.FileGUID
                    } }
                };
            }

            return newContentItem;
        }
    }
}
