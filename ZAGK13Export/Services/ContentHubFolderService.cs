using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAGK13Export.Services
{
    class ContentHubFolderService
    {
        public void AddContentHubFolder(XbyKImport export, string displayName, string name, string parentName)
        {
            if(!ContentHubFolderExists(export, name))
            {
                export.ContentHubFolders.Add(new ContentHubFolder
                {
                    DisplayName = displayName,
                    Name = name,
                    ParentName = parentName
                });
            }
        }

        public bool ContentHubFolderExists(XbyKImport export, string name)
        {
            return export.ContentHubFolders.Where(f => f.Name == name).Any();
        }
    }
}
