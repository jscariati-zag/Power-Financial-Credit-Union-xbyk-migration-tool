using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters
{
    public interface INavigation
    {
        string DisplayName { get; }

        List<Page> Convert();
    }
}
