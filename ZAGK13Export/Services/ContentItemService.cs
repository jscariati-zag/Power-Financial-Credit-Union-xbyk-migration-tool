using CMS.DocumentEngine;
using CMS.Helpers;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Services
{
    class ContentItemService
    {
        private readonly IConfiguration _config;
        private readonly XbyKImport _export;
        private readonly Dictionary<string, IContentItemConverter> _contentItemConverters;
        private readonly ContentHubFolderService _contentHubFolderService;

        public ContentItemService(IConfiguration config, XbyKImport export, IEnumerable<IContentItemConverter> converters, ContentHubFolderService contentHubFolderService)
        {
            _config = config;
            _export = export;
            _contentItemConverters = converters.ToDictionary(c => c.Type, c => c);
            _contentHubFolderService = contentHubFolderService;
        }

        public Task<IEnumerable<TreeNode>> GetContentItems()
        {
            IEnumerable<TreeNode> pages = new MultiDocumentQuery()
                                .OnSite(_config.GetValue<string>("SourceSite"))
                                .Types(_contentItemConverters.Select(c => c.Key).ToArray())
                                .Culture(_config.GetValue<string>("Culture"))
                                .WithCoupledColumns();

            return Task.FromResult(pages);
        }

        public void ConvertContentItems()
        {
            foreach (var contentItem in GetContentItems().Result)
            {
                var convertedContentItem = ConvertContentItem(contentItem.ClassName, contentItem).Result;
                if (convertedContentItem != null) {
                    _export.ContentItems.Add(convertedContentItem);
                }
            }
        }

        public Task<ContentItem> ConvertContentItem(string type, TreeNode contentItem)
        {
            if (_contentItemConverters.TryGetValue(type, out var converter))
            {
                return Task.FromResult(converter.Convert(contentItem));
            }

            throw new ArgumentException($"No content item converter found for type: {type}");
        }

        public void AddContentHubFolders()
        {
            foreach (var contentItemConverter in _contentItemConverters.Select(c => c.Value))
            {
                _contentHubFolderService.AddContentHubFolder(contentItemConverter.FolderDisplayName, contentItemConverter.FolderName, "root");
            }
        }
    }
}
