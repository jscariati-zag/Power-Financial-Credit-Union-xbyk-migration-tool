using CMS.DocumentEngine;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Converters
{
    public interface IContentItemConverter
    {
        string Type { get; }
        string TargetType { get; }
        string FolderDisplayName { get; }
        string FolderName { get; }
        ContentItem Convert(TreeNode input);
    }
}
