using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters
{
    public interface IWidgetConverter
    {
        string Type { get; }
        string TargetType { get; }
        Widget Convert(TreeNode input);
    }
}
