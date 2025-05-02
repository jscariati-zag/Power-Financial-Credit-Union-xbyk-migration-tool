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
                    { "Settings_ContactBand_Title", page.GetValue("ContactTitle", "") },
                    { "Settings_ContactBand_WithinOklahoma", page.GetValue("ContactOklahoma", "") },
                    { "Settings_ContactBand_OutsideOklahoma", page.GetValue("ContactOutsideOklahoma", "") },
                    { "Settings_ContactBand_ContactHours", page.GetValue("ContactHours", "") },
                    { "Settings_Footer_FooterSEO", page.GetValue("FooterSeo", "") },
                    { "Settings_Footer_Routing", page.GetValue("FooterRouting", "") },
                    { "Settings_Footer_Links", page.GetValue("FooterLinks", "") },
                    { "Settings_Footer_Copyright", page.GetValue("FooterCopyright", "") },
                    { "Settings_Footer_Cookie", page.GetValue("FooterCookie", "") },
                    { "Settings_Speedbump_Title", page.GetValue("SpeedbumpTitle", "") },
                    { "Settings_Speedbump_Text", page.GetValue("SpeedbumpText", "") },
                    { "Settings_Speedbump_Whitelist", page.GetValue("SpeedbumpWhitelist", "") },
                    { "Settings_Speedbump_TitleSBA", page.GetValue("SpeedbumpTitleSBA", "") },
                    { "Settings_Speedbump_ContentSBA", page.GetValue("SpeedbumpContentSBA", "") },
                    { "Settings_Speedbump_RepLostOrStolenTitle", page.GetValue("SpeedbumpRepLostOrStolenTitle", "") },
                    { "Settings_Speedbump_RepLostOrStolenText", page.GetValue("SpeedbumpRepLostOrStolenText", "") },
                    { "Settings_Speedbump_RepLostOrStolenTrigger", page.GetValue("SpeedbumpRepLostOrStolenTrigger", "") },
                    { "Settings_Speedbump_RepLostOrStolenButtonText", page.GetValue("SpeedbumpReportLostOrStolenButtonText", "") },
                    { "Settings_Speedbump_RepLostOrStolenButtonArialabel", page.GetValue("SpeedbumpReportLostOrStolenButtonArialabel", "") },
                    { "Settings_Speedbump_FDICTitle", page.GetValue("SpeedbumpFDICTitle", "") },
                    { "Settings_Speedbump_FDICText", page.GetValue("SpeedbumpFDICText", "") },
                    { "Settings_Speedbump_FDICTrigger", page.GetValue("SpeedbumpFDICTrigger", "") }
                }
            };

            return newPage;
        }
    }
}
