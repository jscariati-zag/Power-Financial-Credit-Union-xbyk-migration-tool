using CMS.DocumentEngine;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters.Pages
{
    class PageConverterSettings : IPageConverter
    {
        public string Type => "custom.Settings";
        public string TargetType => "Custom.Page_Settings";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;

        public PageConverterSettings(IConfiguration config, FieldConverters fieldConverters)
        {
            _config = config;
            _fieldConverters = fieldConverters;
        }

        public Page Convert(TreeNode page)
        {
            var newPage = new Page
            {
                OldGuid = page.NodeGUID,
                Type = "Page",
                DisplayName = page.DocumentName,
                ContentType = TargetType,
                Language = _config.GetValue<string>("TargetLanguage"),
                UrlSlug = page.NodeAlias,
                Order = page.NodeOrder,
                Published = page.IsPublished,
                ItemData = new Dictionary<string, object>
                {
                    { "WebPage_Content_Name", page.DocumentName }
                }
            };

            return newPage;
        }
    }
}
