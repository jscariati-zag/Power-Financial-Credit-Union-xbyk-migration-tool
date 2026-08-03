//using AngleSharp.Dom;
//using CMS.DocumentEngine;
//using CMS.DocumentEngine.Routing;
//using CMS.Globalization;
//using Common;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Org.BouncyCastle.Math.EC.ECCurve;

//namespace ZAGK13Export.Converters.Pages
//{
//    class PageConverterSettings : IPageConverter
//    {
//        public string Type => "custom.Settings";
//        public string TargetType => "Custom.Page_Settings";
//        private readonly IConfiguration _config;
//        private readonly FieldConverters _fieldConverters;

//        public PageConverterSettings(IConfiguration config, FieldConverters fieldConverters)
//        {
//            _config = config;
//            _fieldConverters = fieldConverters;
//        }

//        public Page Convert(TreeNode page)
//        {
//            var alerts = DocumentHelper.GetDocuments("custom.Alert")
//                            .Path(page.NodeAliasPath, PathTypeEnum.Section)
//                            .OrderBy(c => c.NodeOrder)
//                            .Select(r => new ContentReference
//                            {
//                                OldGuid = r.NodeGUID
//                            }).ToList();

//            var newPage = new Page
//            {
//                OldGuid = page.NodeGUID,
//                Type = "Page",
//                DisplayName = page.DocumentName,
//                ContentType = TargetType,
//                Language = _config.GetValue<string>("TargetLanguage"),
//                Order = page.NodeOrder,
//                Published = page.IsPublished,
//                ItemData = new Dictionary<string, object>
//                {
//                    { "Alerts", alerts },
//                    { "ContactBand_Title", page.GetValue("ContactTitle", "") },
//                    { "ContactBand_WithinOklahoma", page.GetValue("ContactOklahoma", "") },
//                    { "ContactBand_OutsideOklahoma", page.GetValue("ContactOutsideOklahoma", "") },
//                    { "ContactBand_ContactHours", page.GetValue("ContactHours", "") },
//                    { "Footer_FooterSEO", page.GetValue("FooterSeo", "") },
//                    { "Footer_Routing", page.GetValue("FooterRouting", "") },
//                    { "Footer_Links", page.GetValue("FooterLinks", "") },
//                    { "Footer_Copyright", page.GetValue("FooterCopyright", "") },
//                    { "Footer_Cookie", page.GetValue("FooterCookie", "") },
//                    { "Speedbump_Title", page.GetValue("SpeedbumpTitle", "") },
//                    { "Speedbump_Text", page.GetValue("SpeedbumpText", "") },
//                    { "Speedbump_Whitelist", page.GetValue("SpeedbumpWhitelist", "") },
//                    { "Speedbump_TitleSBA", page.GetValue("SpeedbumpTitleSBA", "") },
//                    { "Speedbump_ContentSBA", page.GetValue("SpeedbumpContentSBA", "") },
//                    { "Speedbump_RepLostOrStolenTitle", page.GetValue("SpeedbumpRepLostOrStolenTitle", "") },
//                    { "Speedbump_RepLostOrStolenText", page.GetValue("SpeedbumpRepLostOrStolenText", "") },
//                    { "Speedbump_RepLostOrStolenTrigger", page.GetValue("SpeedbumpRepLostOrStolenTrigger", "") },
//                    { "Speedbump_RepLostOrStolenButtonText", page.GetValue("SpeedbumpReportLostOrStolenButtonText", "") },
//                    { "Speedbump_RepLostOrStolenButtonArialabel", page.GetValue("SpeedbumpReportLostOrStolenButtonArialabel", "") },
//                    { "Speedbump_FDICTitle", page.GetValue("SpeedbumpFDICTitle", "") },
//                    { "Speedbump_FDICText", page.GetValue("SpeedbumpFDICText", "") },
//                    { "Speedbump_FDICTrigger", page.GetValue("SpeedbumpFDICTrigger", "") }
//                }
//            };

//            return newPage;
//        }
//    }
//}
