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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters
{
    class AttachmentConverter
    {
        private readonly IConfiguration _config;

        public AttachmentConverter(IConfiguration config)
        {
            _config = config;
        }

        public ContentItem Convert(AttachmentInfo attachment)
        {
            ContentItem? newContentItem = new ContentItem();

            if (_config.GetValue<string>("ImageExtensions").Split(';').Contains(attachment.AttachmentExtension.TrimStart('.')))
            {
                newContentItem.OldGuid = attachment.AttachmentGUID;
                newContentItem.DisplayName = attachment.AttachmentName;
                newContentItem.ContentType = "Custom.Reusable_Image";
                newContentItem.Language = _config.GetValue<string>("TargetLanguage");
                newContentItem.Published = true;
                newContentItem.ItemData = new Dictionary<string, object>
                {
                    { "Description", attachment.AttachmentDescription },
                    { "Asset_Image", new Asset{
                        AssetUrl = _config.GetValue<string>("BaseUrl") + Regex.Replace(AttachmentURLProvider.GetAttachmentUrl(attachment.AttachmentGUID, attachment.AttachmentName), "^~", ""),
                        FileGuid = attachment.AttachmentGUID
                    } }
                };
            }
            else if (_config.GetValue<string>("DocumentExtensions").Split(';').Contains(attachment.AttachmentExtension.TrimStart('.')))
            {
                newContentItem.OldGuid = attachment.AttachmentGUID;
                newContentItem.DisplayName = attachment.AttachmentName;
                newContentItem.ContentType = "Custom.Reusable_Document";
                newContentItem.Language = _config.GetValue<string>("TargetLanguage");
                newContentItem.Published = true;
                newContentItem.ItemData = new Dictionary<string, object>
                {
                    { "Description", attachment.AttachmentDescription },
                    { "Asset_Document", new Asset{
                        AssetUrl = _config.GetValue<string>("BaseUrl") + Regex.Replace(AttachmentURLProvider.GetAttachmentUrl(attachment.AttachmentGUID, attachment.AttachmentName), "^~", ""),
                        FileGuid = attachment.AttachmentGUID
                    } }
                };
            }

            return newContentItem;
        }
    }
}
