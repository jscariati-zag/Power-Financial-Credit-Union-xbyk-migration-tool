using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ZAGXbyKImport.Services;

// Ensures a preconfigured environment (DI, configuration providers) for .NET console apps
var builder = Host.CreateApplicationBuilder(args);
string json = File.ReadAllText(builder.Configuration.GetValue<string>("ImportFilePath"));

XbyKImport import = JsonConvert.DeserializeObject<XbyKImport>(json, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto
});

builder.Services.AddSingleton(import);
builder.Services.AddSingleton<ImportService>();
builder.Services.AddSingleton<CommonFunctionsService>();
builder.Services.AddSingleton<ContentHubFolderService>();
builder.Services.AddSingleton<ContentItemService>();
builder.Services.AddSingleton<PageService>();

// Preinitializes Xperience by Kentico without building a custom DI container
CMSApplication.PreInit(false);
// Merges Xperience services with the application's service collection
Service.MergeDescriptors(builder.Services);

var app = builder.Build();

Console.WriteLine("Program Start...");

// Tells Xperience to use this app's service container for service resolution
Service.SetProvider(app.Services);

// Initializes Xperience APIs and database for use in the app
CMSApplication.Init();

using (var scope = app.Services.CreateScope())
{
    var importService = scope.ServiceProvider.GetRequiredService<ImportService>();
    await importService.RunAsync();
}