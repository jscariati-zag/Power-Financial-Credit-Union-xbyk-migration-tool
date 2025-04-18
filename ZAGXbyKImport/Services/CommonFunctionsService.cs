using CMS.ContentEngine;
using CMS.DataEngine;
using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGXbyKImport.Services
{
    class CommonFunctionsService
    {
        private readonly IConfiguration config;
        private readonly XbyKImport xbyKImport;

        public CommonFunctionsService(IConfiguration config, XbyKImport xbyKImport)
        {
            this.config = config;
            this.xbyKImport = xbyKImport;
        }

        public WidgetConfiguration ConvertWidgetProperties(WidgetConfiguration widgetConfiguration, bool skipReferences)
        {
            if (widgetConfiguration != null && widgetConfiguration.editableAreas[0] != null)
            {
                foreach (var section in widgetConfiguration.editableAreas[0].sections)
                {
                    foreach (var zone in section.zones)
                    {
                        foreach (var widget in zone.widgets)
                        {
                            foreach (var variant in widget.variants)
                            {
                                variant.properties = ConvertItemData(variant.properties, skipReferences);
                            }
                        }
                    }
                }
            }
            return widgetConfiguration;
        }

        public Dictionary<string, object> ConvertItemData(Dictionary<string, object> itemData, bool skipReferences)
        {
            var mediaPath = config.GetValue<string>("MediaPath");
            Dictionary<string, object> fields = new Dictionary<string, object>();

            foreach (var item in itemData)
            {
                switch (item.Value)
                {
                    case Asset:
                        var asset = item.Value as Asset;
                        var mediaFilePath = mediaPath + asset.AssetPath;
                        if (!File.Exists(mediaFilePath)) { break; }
                        var file = CMS.IO.FileInfo.New(mediaFilePath);
                        var assetMetadata = new ContentItemAssetMetadata()
                        {
                            Extension = file.Extension,
                            Identifier = asset.FileGuid,
                            LastModified = DateTime.Now,
                            Name = file.Name,
                            Size = file.Length
                        };
                        var fileSource = new ContentItemAssetFileSource(file.FullName, false);
                        var assetMetadataWithSource = new ContentItemAssetMetadataWithSource(fileSource, assetMetadata);
                        fields.Add(item.Key, assetMetadataWithSource);

                        break;
                    case ContentReference:
                        if(skipReferences) { break; }
                        List<ContentItemReference> contentReferencelist = new List<ContentItemReference>();
                        var contentReference = item.Value as ContentReference;
                        var contentReferenceContentItem = xbyKImport.ContentItems.Where(c => c.OldGuid == contentReference.OldGuid).FirstOrDefault();
                        if (contentReferenceContentItem != null)
                        {
                            contentReferencelist.Add(new ContentItemReference
                            {
                                Identifier = contentReferenceContentItem.ContentItemGUID
                            });
                        }
                        fields.Add(item.Key, contentReferencelist);
                        break;
                    case List<ContentReference>:
                        if (skipReferences) { break; }
                        List<ContentItemReference> contentReferencesList = new List<ContentItemReference>();
                        var contentReferences = item.Value as List<ContentReference>;
                        foreach (var contentReferencesItem in contentReferences)
                        {
                            var contentReferencesContentItem = xbyKImport.ContentItems.Where(c => c.OldGuid == contentReferencesItem.OldGuid).FirstOrDefault();
                            if (contentReferencesContentItem != null)
                            {
                                contentReferencesList.Add(new ContentItemReference
                                {
                                    Identifier = contentReferencesContentItem.ContentItemGUID
                                });
                            }
                        }
                        fields.Add(item.Key, contentReferencesList);
                        break;
                    case PageReference:
                        if (skipReferences) { break; }
                        List<ContentItemReference> pageReferencelist = new List<ContentItemReference>();
                        var pageReference = item.Value as PageReference;
                        var pageReferenceContentItem = xbyKImport.Pages.Where(p => p.OldGuid == pageReference.OldGuid).FirstOrDefault();
                        if (pageReferenceContentItem != null)
                        {
                            pageReferencelist.Add(new ContentItemReference
                            {
                                Identifier = pageReferenceContentItem.WebPageItemGUID
                            });
                        }
                        fields.Add(item.Key, pageReferencelist);
                        break;
                    case List<PageReference>:
                        if (skipReferences) { break; }
                        List<ContentItemReference> pageReferencesList = new List<ContentItemReference>();
                        var pageReferences = item.Value as List<PageReference>;
                        foreach (var pageReferencesItem in pageReferences)
                        {
                            var pageReferencesContentItem = xbyKImport.Pages.Where(c => c.OldGuid == pageReferencesItem.OldGuid).FirstOrDefault();
                            if (pageReferencesContentItem != null)
                            {
                                pageReferencesList.Add(new ContentItemReference
                                {
                                    Identifier = pageReferencesContentItem.WebPageItemGUID
                                });
                            }
                        }
                        fields.Add(item.Key, pageReferencesList);
                        break;
                    case string:
                        var itemValue = item.Value as string;
                        fields.Add(item.Key, ConvertTextReferences(itemValue));
                        break;
                    default:
                        fields.Add(item.Key, item.Value);
                        break;
                }
            }

            return fields;
        }

        public string ConvertTextReferences(string itemValue)
        {
            itemValue = ConvertMediaUrls(itemValue);
            return itemValue;
        }

        public string ConvertMediaUrls(string itemValue)
        {
            var pattern = @"""~?\/getmedia\/(?<guid>[0-9a-fA-F\-]{36})\/(?<filename>[^""/?]+)(?:\?[^""]*)?""";

            var match = Regex.Match(itemValue, pattern);

            if (match.Success)
            {
                Guid oldGuid = Guid.Parse(match.Groups["guid"].Value);
                string filename = match.Groups["filename"].Value;

                var contentItem = xbyKImport.ContentItems.Where(c => c.OldGuid == oldGuid).FirstOrDefault();

                string replacement = "";
                if (contentItem != null)
                {
                    replacement = $"~/getContentAsset/{contentItem.ContentItemGUID.ToString()}/{config.GetValue<string>("ImageAssetFieldGUID")}/{filename}?language={contentItem.Language}";
                }

                itemValue = Regex.Replace(itemValue, pattern, replacement);
            }

            return itemValue;
        }
    }
}
