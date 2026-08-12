using Amazon.Runtime.Internal.Transform;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Services;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Converters.Pages
{
    class PageConverterBlogYear : IPageConverter
    {
        public string Type => "custom.BlogYear";
        public string TargetType => "Custom.Page_Group";
        private readonly IConfiguration _config;
        private readonly FieldConverters _fieldConverters;
        private readonly CommonConverterService _commonConverterService;

        public PageConverterBlogYear(IConfiguration config, FieldConverters fieldConverters, CommonConverterService commonConverterService)
        {
            _config = config;
            _fieldConverters = fieldConverters;
            _commonConverterService = commonConverterService;
        }

        public Page? Convert(TreeNode page)
        {
            // The target site combines blog year/month into a single "Month Year" group page
            // (see PageConverterBlogMonth), so BlogYear nodes are not created as pages themselves.
            // Returning null flattens this node out of the exported tree; its children (BlogMonth
            // nodes) are still processed and attached directly to this node's parent.
            return null;
        }
    }
}
