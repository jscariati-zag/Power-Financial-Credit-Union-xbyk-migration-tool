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
            var newContentItem = new ContentItem
            {
                OldGuid = attachment.AttachmentGUID,
                DisplayName = attachment.AttachmentName,
                ContentType = "Custom.Reusable_Image",
                Language = _config.GetValue<string>("TargetLanguage"),
                Published = true,
                ItemData = new Dictionary<string, object>
                {
                    { "Description", attachment.AttachmentDescription },
                    { "Image", new Asset{
                        AssetUrl = _config.GetValue<string>("BaseUrl") + Regex.Replace(AttachmentURLProvider.GetAttachmentUrl(attachment.AttachmentGUID, attachment.AttachmentName), "^~", ""),
                        FileGuid = attachment.AttachmentGUID
                    } }
                }
            };

            return newContentItem;
        }
    }
}
