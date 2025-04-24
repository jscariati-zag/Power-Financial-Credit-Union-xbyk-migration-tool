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
        private readonly XbyKImport _export;

        public ContentHubFolderService(XbyKImport export) { 
            _export = export;
        }

        public void AddContentHubFolder(string displayName, string name, string parentName)
        {
            if(!ContentHubFolderExists(name))
            {
                _export.ContentHubFolders.Add(new ContentHubFolder
                {
                    DisplayName = displayName,
                    Name = name,
                    ParentName = parentName
                });
            }
        }

        public bool ContentHubFolderExists(string name)
        {
            return _export.ContentHubFolders.Where(f => f.Name == name).Any();
        }
    }
}
