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

            return new ContentReference
            {
                OldGuid = mediaGuid
            };
        }

        public string ConvertCtas(string ctas)
        {
            var ctasAr = ctas.Split("\r\n");

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
    }
}
