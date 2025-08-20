using AngleSharp.Dom;
using Azure;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using URLRedirection;
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
                foreach (var editableArea in widgetConfiguration.editableAreas)
                {
                    foreach (var section in editableArea.sections)
                    {
                        section.properties = ConvertItemData(section.properties, skipReferences);

                        foreach (var zone in section.zones)
                        {
                            foreach (var widget in zone.widgets)
                            {
                                if (widget != null)
                                {
                                    foreach (var variant in widget.variants)
                                    {
                                        variant.properties = ConvertItemData(variant.properties, skipReferences);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return widgetConfiguration;
        }

        public static async Task<MemoryStream?> GetStreamFromUrlAsync(string url)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (HttpRequestMessage msg, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors) => true
            };

            using (HttpClient client = new HttpClient(handler))
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode) // i.e., 200–299
                {
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    return new MemoryStream(data);
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddMediaRedirect(string oldUrl, string newUrl, string type)
        {
            var provider = RedirectionTableInfo.Provider;
            var redirect = new RedirectionTableInfo
            {
                RedirectionEnabled = true,
                RedirectionOriginalURL = oldUrl,
                RedirectionTargetURL = newUrl,
                RedirectionType = type,
                RedirectionSiteID = config.GetValue<int>("WebsiteChannelID"),
                RedirectionMigrated = false
            };

            provider.BulkInsert([redirect]);
        }

        public Dictionary<string, object> ConvertItemData(Dictionary<string, object> itemData, bool skipReferences, ContentItem? contentItem = null)
        {
            var imageAssetFieldGuid = config.GetValue<string>("ImageAssetFieldGUID");
            var documentAssetFieldGuid = config.GetValue<string>("DocumentAssetFieldGUID");
            Dictionary<string, object> fields = new Dictionary<string, object>();

            foreach (var item in itemData)
            {
                switch (item.Value)
                {
                    case Asset:
                        var asset = item.Value as Asset;
                        var stream = GetStreamFromUrlAsync(asset.AssetUrl).Result;
                        if (stream == null) { break; }
                        var filename = Path.GetFileName(new Uri(asset.AssetUrl).AbsolutePath);
                        var assetMetadata = new ContentItemAssetMetadata()
                        {
                            Extension = Path.GetExtension(new Uri(asset.AssetUrl).AbsolutePath),
                            Identifier = asset.FileGuid,
                            LastModified = DateTime.Now,
                            Name = filename,
                            Size = stream.Length
                        };
                        var fileSource = new ContentItemAssetStreamSource((CancellationToken cancellationToken) => Task.FromResult<System.IO.Stream>(stream));
                        var assetMetadataWithSource = new ContentItemAssetMetadataWithSource(fileSource, assetMetadata);
                        fields.Add(item.Key, assetMetadataWithSource);

                        if (!skipReferences)
                        {
                            string? assetFieldGuid = contentItem.ContentType == "Custom.Reusable_Image"
                            ? imageAssetFieldGuid
                            : documentAssetFieldGuid;

                            if (!contentItem.OldDirectUrl.IsNullOrEmpty())
                            {
                                AddMediaRedirect(contentItem.OldDirectUrl, $"/getContentAsset/{contentItem.ContentItemGUID}/{assetFieldGuid}/{filename}?language={contentItem.Language}", "302");
                            }
                        }

                        break;
                    case ContentReference:
                        if (skipReferences) { break; }
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
                        var pageReferenceContentItem = FindPageByOldGuid(xbyKImport.Pages, pageReference.OldGuid);
                        if (pageReferenceContentItem != null)
                        {
                            pageReferencelist.Add(new ContentItemReference
                            {
                                Identifier = pageReferenceContentItem.ContentItemGUID
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
                            var pageReferencesContentItem = FindPageByOldGuid(xbyKImport.Pages, pageReferencesItem.OldGuid);
                            if (pageReferencesContentItem != null)
                            {
                                pageReferencesList.Add(new ContentItemReference
                                {
                                    Identifier = pageReferencesContentItem.ContentItemGUID
                                });
                            }
                        }
                        fields.Add(item.Key, pageReferencesList);
                        break;
                    case RelatedPageReference:
                        if (skipReferences) { break; }
                        List<WebPageRelatedItem> webPageReferencelist = new List<WebPageRelatedItem>();
                        var webPageReference = item.Value as RelatedPageReference;
                        var webPageReferenceContentItem = FindPageByOldGuid(xbyKImport.Pages, webPageReference.OldGuid);
                        if (webPageReferenceContentItem != null)
                        {
                            webPageReferencelist.Add(new WebPageRelatedItem
                            {
                                WebPageGuid = webPageReferenceContentItem.WebPageItemGUID
                            });
                        }
                        fields.Add(item.Key, webPageReferencelist);
                        break;
                    case List<RelatedPageReference>:
                        if (skipReferences) { break; }
                        List<WebPageRelatedItem> webPageReferencesList = new List<WebPageRelatedItem>();
                        var webPageReferences = item.Value as List<RelatedPageReference>;
                        foreach (var pageReferencesItem in webPageReferences)
                        {
                            var pageReferencesContentItem = FindPageByOldGuid(xbyKImport.Pages, pageReferencesItem.OldGuid);
                            if (pageReferencesContentItem != null)
                            {
                                webPageReferencesList.Add(new WebPageRelatedItem
                                {
                                    WebPageGuid = pageReferencesContentItem.WebPageItemGUID
                                });
                            }
                        }
                        fields.Add(item.Key, webPageReferencesList);
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

        public static Page? FindPageByOldGuid(List<Page> pages, Guid oldGuid)
        {
            foreach (var page in pages)
            {
                if (page.OldGuid == oldGuid)
                {
                    return page;
                }
                else if (page.Children != null && page.Children.Any())
                {
                    var result = FindPageByOldGuid(page.Children, oldGuid);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public string ConvertTextReferences(string itemValue)
        {
            var imageAssetFieldGuid = config.GetValue<string>("ImageAssetFieldGUID");
            var documentAssetFieldGuid = config.GetValue<string>("DocumentAssetFieldGUID");
            var sourceSite = config.GetValue<string>("SourceSite");
            itemValue = ConvertMediaUrls(itemValue, imageAssetFieldGuid, documentAssetFieldGuid, sourceSite);
            itemValue = ConvertAttachmentUrls(itemValue, imageAssetFieldGuid, documentAssetFieldGuid);
            return itemValue;
        }

        public string ConvertMediaUrls(string itemValue, string imageAssetFieldGuid, string documentAssetFieldGuid, string sourceSite)
        {
            var pattern = @"(?<="")~?/getmedia/(?<guid>[0-9a-fA-F\-]{36})/(?<filename>[^""/?]+)(?:\?[^""]*)?(?="")";

            itemValue = Regex.Replace(itemValue, pattern, match =>
            {
                Guid oldGuid = Guid.Parse(match.Groups["guid"].Value);
                string filename = match.Groups["filename"].Value;

                var contentItem = xbyKImport.ContentItems.FirstOrDefault(c => c.OldGuid == oldGuid);
                if (contentItem != null)
                {
                    string? assetFieldGuid = contentItem.ContentType == "Custom.Reusable_Image"
                        ? imageAssetFieldGuid
                        : documentAssetFieldGuid;

                    return $"~/getContentAsset/{contentItem.ContentItemGUID}/{assetFieldGuid}/{filename}?language={contentItem.Language}";
                }

                return match.Value; // fallback to original if not found
            });

            // check for direct path matches

            var directPattern = $@"(?<="")~?/{Regex.Escape(sourceSite)}/media/(?:[^""/]+/)*(?<filename>[^""/]+)(?="")";

            itemValue = Regex.Replace(itemValue, directPattern, match =>
            {
                string filename = match.Groups["filename"].Value;

                var contentItem = xbyKImport.ContentItems.FirstOrDefault(c => c.OldDirectUrl == match.Value.TrimStart('~'));
                if (contentItem != null)
                {
                    string? assetFieldGuid = contentItem.ContentType == "Custom.Reusable_Image"
                        ? imageAssetFieldGuid
                        : documentAssetFieldGuid;

                    return $"~/getContentAsset/{contentItem.ContentItemGUID}/{assetFieldGuid}/{filename}?language={contentItem.Language}";
                }

                return match.Value; // fallback to original if not found
            });

            return itemValue;
        }

        public string ConvertAttachmentUrls(string itemValue, string imageAssetFieldGuid, string documentAssetFieldGuid)
        {
            var pattern = @"(?<="")~?/getattachment/(?<guid>[0-9a-fA-F\-]{36})/(?<filename>[^""/?]+)(?:\?[^""]*)?(?="")";

            itemValue = Regex.Replace(itemValue, pattern, match =>
            {
                Guid oldGuid = Guid.Parse(match.Groups["guid"].Value);
                string filename = match.Groups["filename"].Value;

                var contentItem = xbyKImport.ContentItems.FirstOrDefault(c => c.OldGuid == oldGuid);
                if (contentItem != null)
                {
                    string? assetFieldGuid = contentItem.ContentType == "Custom.Reusable_Image"
                        ? imageAssetFieldGuid
                        : documentAssetFieldGuid;

                    return $"~/getContentAsset/{contentItem.ContentItemGUID}/{assetFieldGuid}/{filename}?language={contentItem.Language}";
                }

                return match.Value; // fallback to original if not found
            });

            return itemValue;
        }
    }
}
