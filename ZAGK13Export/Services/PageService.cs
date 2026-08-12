using CMS.DocumentEngine;
using CMS.Helpers;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Services
{
    class PageService
    {
        private readonly IConfiguration _config;
        private readonly Dictionary<string, IPageConverter> _pageConverters;

        public PageService(IConfiguration config, IEnumerable<IPageConverter> converters)
        {
            _config = config;
            _pageConverters = converters.ToDictionary(c => c.Type, c => c);
        }

        public Task<IEnumerable<TreeNode>> GetPages(TreeNode parent)
        {
            IEnumerable<TreeNode> pages;

            ////write out the page types we're getting
            //foreach (var converter in _pageConverters)
            //{
            //    //Console.WriteLine($"Converter value: {converter.Value}");
            //    Console.WriteLine($"\n Converter key: {converter.Key}");
            //}

            if (parent == null)
            {
                pages = new MultiDocumentQuery()
                                .OnSite(_config.GetValue<string>("SourceSite"))
                                .Types(_pageConverters.Select(c => c.Key).ToArray())
                                .Culture(_config.GetValue<string>("Culture"))
                                .WithCoupledColumns()
                                .NestingLevel(1);
            }
            else
            {
                pages = new MultiDocumentQuery()
                                .Path(parent.NodeAliasPath, PathTypeEnum.Children)
                                .OnSite(_config.GetValue<string>("SourceSite"))
                                .Types(_pageConverters.Select(c => c.Key).ToArray())
                                .Culture(_config.GetValue<string>("Culture"))
                                .WithCoupledColumns()
                                .NestingLevel(1);
            }

            return Task.FromResult(pages);
        }

        public void ConvertPages(List<Page> pages, TreeNode parent)
        {
            foreach (var page in GetPages(parent).Result)
            {
                if (_config.GetSection("SkipPaths").Get<string[]>() == null || !_config.GetSection("SkipPaths").Get<string[]>().Any(s => page.NodeAliasPath.StartsWith(s))) {
                    var convertedPage = ConvertPage(page.ClassName, page).Result;

                    if (convertedPage == null)
                    {
                        // Pass-through node (e.g. BlogYear): don't create a page for it, but still
                        // process its children and attach them directly to the current parent level,
                        // effectively flattening this node out of the exported tree.
                        if (page.NodeHasChildren)
                        {
                            ConvertPages(pages, page);
                        }
                        continue;
                    }

                    pages.Add(convertedPage);

                    if (page.NodeHasChildren)
                    {
                        ConvertPages(convertedPage.Children = new List<Page>(), page);
                    }
                }
            }
        }

        public Task<Page?> ConvertPage(string type, TreeNode page)
        {
            if (_pageConverters.TryGetValue(type, out var converter))
            {
                return Task.FromResult(converter.Convert(page));
            }

            throw new ArgumentException($"No page converter found for type: {type}");
        }
    }
}
