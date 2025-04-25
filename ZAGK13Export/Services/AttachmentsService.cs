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
    class AttachmentsService
    {
        private readonly IConfiguration _config;
        private readonly XbyKImport _export;
        private readonly AttachmentConverter _attachmentConverter;
        private readonly ContentHubFolderService _contentHubFolderService;

        public AttachmentsService(IConfiguration config, XbyKImport export, AttachmentConverter attachmentConverter, ContentHubFolderService contentHubFolderService)
        {
            _config = config;
            _export = export;
            _attachmentConverter = attachmentConverter;
            _contentHubFolderService = contentHubFolderService;
        }

        public Task<IEnumerable<AttachmentInfo>> GetAttachments()
        {
            var attachments = AttachmentInfo.Provider.Get().AsEnumerable();

            return Task.FromResult(attachments);
        }

        public void ConvertAttachments()
        {
            foreach (var attachmentInfo in GetAttachments().Result)
            {
                _export.ContentItems.Add(ConvertAttachment(attachmentInfo).Result);
            }
        }

        public Task<ContentItem> ConvertAttachment(AttachmentInfo attachment)
        {
            return Task.FromResult(_attachmentConverter.Convert(attachment));
        }
    }
}
