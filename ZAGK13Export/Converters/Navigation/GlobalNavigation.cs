using Common;
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

        public List<Page> Pages => new List<Page>();
    }
}
