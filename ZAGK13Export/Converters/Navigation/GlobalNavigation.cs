using Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13Export.Converters;

namespace ZAGK13Export.Converters.Navigation
{
    class GlobalNavigation : INavigation
    {
        public string DisplayName => "Global Nav Items";

        private readonly IConfiguration _config;

        public GlobalNavigation(IConfiguration config)
        {
            _config = config;
        }

        public List<Page> Convert()
        {
            return new List<Page>();
        }
    }
}
