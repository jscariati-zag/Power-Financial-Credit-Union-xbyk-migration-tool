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
using ZAGK13Export.Converters.Navigation;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ZAGK13Export.Services
{
    class NavigationService
    {
        private readonly IConfiguration _config;

        public NavigationService(IConfiguration config)
        {
            _config = config;
        }

        public void AddNavigation(List<Page> pages)
        {
            var navigationParentGUID = _config.GetValue<string>("NavigationParentGUID");
            var targetLanguage = _config.GetValue<string>("TargetLanguage");

            if (navigationParentGUID != null)
            {
                var parentPage = pages.FirstOrDefault(p => p.OldGuid == Guid.Parse(navigationParentGUID));

                if (parentPage != null)
                {
                    var navigation = new List<Page>();
                    int i = 1;

                    foreach(var navigationRegistryItem in NavigationRegistry.All)
                    {
                        navigation.Add(new Page
                        {
                            DisplayName = navigationRegistryItem.DisplayName,
                            Type = "Folder",
                            Language = targetLanguage,
                            Order = i,
                        });
                        i++;
                    }

                    parentPage.Children.AddRange(navigation);
                }
            }
        }
    }
}
