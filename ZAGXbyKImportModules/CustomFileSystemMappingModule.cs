using CMS;
using CMS.DataEngine;
using CMS.IO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGXbyKImport.Modules;

[assembly: RegisterModule(typeof(CustomFileSystemMappingModule))]
namespace ZAGXbyKImport.Modules
{
    public class CustomFileSystemMappingModule : Module
    {
        // Module class constructor, the system registers the module under the name "CustomFSMapping"
        public CustomFileSystemMappingModule()
            : base("CustomFSMapping")
        {
        }

        // Contains initialization code that is executed when the application starts
        protected override void OnInit()
        {
            base.OnInit();

            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Creates a new StorageProvider instance for regular file systems (NTFS, VFAT)
            var sharedAssetsProvider = StorageProvider.CreateFileSystemStorageProvider(isSharedStorage: true);

            // Specifies the root directory of the target Xperience project.
            // The provider creates the relative path of the mapped folders within the given directory.
            sharedAssetsProvider.CustomRootPath = config["ApplicationRootPath"];

            // Maps the ~/assets directory from your external application's folder to the provider
            StorageHelper.MapStoragePath("~/assets/", sharedAssetsProvider);
        }
    }
}
