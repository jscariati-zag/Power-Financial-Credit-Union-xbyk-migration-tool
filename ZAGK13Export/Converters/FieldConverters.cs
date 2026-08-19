using CMS.MediaLibrary;
using Common;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZAGK13Export.Models;
using ZAGK13Export.Services;

namespace ZAGK13Export.Converters
{
    class FieldConverters
    {
       
        public FieldConverters()
        {

        }

        public ContentReference ConvertMediaItemReference(string mediaUrl)
        {
            Guid mediaGuid = Guid.NewGuid();
            string pattern = @"([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})";

            Match match = Regex.Match(mediaUrl, pattern);

            if (match.Success)
            {
                mediaGuid = Guid.Parse(match.Value);
            }
            else
            {
                // Some fields (e.g. BannerImage) reference media files via a direct file path URL
                // instead of a GUID-based getmedia URL, e.g.
                // "/PowerFI/media/Images/Promo Images/Full Width Image Promos/Business-merchant-services.jpg?ext=.jpg"
                // In that case, resolve the file by matching its library folder + relative path
                // against the media library file records.
                var resolvedGuid = ResolveMediaFileGuidFromDirectUrl(mediaUrl);
                if (resolvedGuid.HasValue)
                {
                    mediaGuid = resolvedGuid.Value;
                }
            }

            return new ContentReference
            {
                OldGuid = mediaGuid
            };
        }

        private static Guid? ResolveMediaFileGuidFromDirectUrl(string mediaUrl)
        {
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            // Strip any query string (e.g. "?ext=.jpg") and decode URL-encoded characters
            // (e.g. "%20" -> " ").
            string path = mediaUrl.Split('?')[0];
            path = Uri.UnescapeDataString(path);

            string[] segments = path.Trim('/').Split('/');

            int mediaIndex = Array.FindIndex(segments, s => string.Equals(s, "media", StringComparison.OrdinalIgnoreCase));
            if (mediaIndex < 0 || mediaIndex + 1 >= segments.Length)
            {
                return null;
            }

            // Segment immediately after "media" is the library folder; everything after that is
            // the file's relative path within the library.
            string libraryFolder = segments[mediaIndex + 1];
            string[] relativeSegments = segments.Skip(mediaIndex + 2).ToArray();
            if (relativeSegments.Length == 0)
            {
                return null;
            }

            string relativePath = string.Join("/", relativeSegments);

            var mediaLibrary = MediaLibraryInfo.Provider.Get()
                .WhereEquals(nameof(MediaLibraryInfo.LibraryFolder), libraryFolder)
                .FirstOrDefault();

            if (mediaLibrary == null)
            {
                return null;
            }

            var mediaFile = MediaFileInfo.Provider.Get()
                .WhereEquals(nameof(MediaFileInfo.FileLibraryID), mediaLibrary.LibraryID)
                .WhereEquals(nameof(MediaFileInfo.FilePath), relativePath)
                .FirstOrDefault();

            return mediaFile?.FileGUID;
        }

        public string ConvertCtas(string ctas)
        {
            var ctasAr = ctas.Split(["\r\n", "\n"], StringSplitOptions.None);

            if (ctasAr.Length == 0)
            {
                return "";
            }

            var zagJsonTable = new ZAGJsonTable();

            foreach (var cta in ctasAr)
            {
                var ctaAr = cta.Split('|');

                if (ctaAr.Length == 4)
                {
                    zagJsonTable.rows.Add(new ZAGJsonTableRows
                    {
                        guid = Guid.NewGuid().ToString(),
                        type = "Unset",
                        fields = new List<ZAGJsonTableField>
                        {
                            { new ZAGJsonTableField {
                                name = "Text",
                                slug = "text",
                                type = "Text",
                                value = ctaAr[0],
                                options = new List<string>(),
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }},
                            { new ZAGJsonTableField {
                                name = "Url",
                                slug = "url",
                                type = "Text",
                                value = ctaAr[1],
                                options = new List<string>(),
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }},
                            { new ZAGJsonTableField {
                                name = "Target",
                                slug = "target",
                                type = "Dropdown",
                                value = ctaAr[2],
                                options = new List<string>()
                                {
                                    "_self",
                                    "_blank"
                                },
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }},
                            { new ZAGJsonTableField {
                                name = "Aria label",
                                slug = "ariaLabel",
                                type = "Text",
                                value = ctaAr[3],
                                options = new List<string>(),
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }},
                        }
                    });
                }
            }

            return JsonSerializer.Serialize(zagJsonTable.rows);
        }

        public string ConvertFeatures(string features)
        {
            var featuresAr = features.Split(["\r\n", "\n"], StringSplitOptions.None);

            if (featuresAr.Length == 0)
            {
                return "";
            }

            var zagJsonTable = new ZAGJsonTable();

            foreach (var feature in featuresAr)
            {
                var featureAr = feature.Split('|');

                if (featureAr.Length == 2)
                {
                    zagJsonTable.rows.Add(new ZAGJsonTableRows
                    {
                        guid = Guid.NewGuid().ToString(),
                        type = "Unset",
                        fields = new List<ZAGJsonTableField>
                        {
                            { new ZAGJsonTableField {
                                name = "Label",
                                slug = "label",
                                type = "Text",
                                value = featureAr[0],
                                options = new List<string>(),
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }},
                            { new ZAGJsonTableField {
                                name = "Icon",
                                slug = "icon",
                                type = "Text",
                                value = featureAr[1],
                                options = new List<string>(),
                                visible = true,
                                tableRowSpan = false,
                                tableColSpan = false,
                                tableRowSpanCount = 0,
                                tableColSpanCount = 0
                            }}
                        }
                    });
                }
            }

            return JsonSerializer.Serialize(zagJsonTable.rows);
        }
    }
}
