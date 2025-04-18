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
        public static readonly INavigation PrimaryNavigation = new PrimaryNavigation();
        public static readonly INavigation GlobalNavigation = new GlobalNavigation();
        public static readonly INavigation FooterNavigation = new FooterNavigation();

        public static readonly List<INavigation> All = new List<INavigation>
        {
            PrimaryNavigation,
            GlobalNavigation,
            FooterNavigation
        };
    }
}
