using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;

namespace ZAGK13Export.Converters.Navigation
{
    public static class NavigationRegistry
    {
        private static readonly IConfiguration _config;

        public static readonly INavigation PrimaryNavigation;
        public static readonly INavigation GlobalNavigation;
        public static readonly INavigation FooterNavigation;

        public static readonly List<INavigation> All;

        static NavigationRegistry()
        {
            // Initialize configuration once
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Pass config to constructors
            PrimaryNavigation = new PrimaryNavigation(_config);
            GlobalNavigation = new GlobalNavigation(_config);
            FooterNavigation = new FooterNavigation(_config);

            All = new List<INavigation>
            {
                PrimaryNavigation,
                GlobalNavigation,
                FooterNavigation
            };
        }
    }
}
