using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters
{
    public interface IPageConverter
    {
        string Type { get; }
        string TargetType { get; }
        Page Convert(TreeNode input);
    }
}
