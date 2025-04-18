using CMS;
using CMS.Core;
using CMS.DataEngine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZAGK13ExportModules;

[assembly: RegisterModule(typeof(CustomInitModule))]
namespace ZAGK13ExportModules
{
    class CustomInitModule : Module
    {
        public CustomInitModule() : base("CustomInit")
        {
        }

        protected override void OnPreInit()
        {
            // Registers a configuration provider configured to look
            // for application settings in an 'appsettings.json' file.
            // This overrides the default registration.
            Service.Use<IConfiguration>(() => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build());
        }
    }
}
