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
        // Returns null to indicate the source node should be treated as a "pass-through" -
        // no page is created for it in the target, but its children are still processed
        // and attached to the current node's parent (flattening this level out of the tree).
        Page? Convert(TreeNode input);
    }
}
