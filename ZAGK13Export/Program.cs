using CMS.DataEngine;
using CMS.DocumentEngine;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ZAGK13Export.Converters;
using ZAGK13Export.Converters.ContentItems;
using ZAGK13Export.Converters.Pages;
using ZAGK13Export.Services;

var builder = Host.CreateApplicationBuilder(args);

XbyKImport export = new XbyKImport();

builder.Services.AddSingleton(export);
builder.Services.AddSingleton<CommonConverterService>();
builder.Services.AddSingleton<ExportService>();
builder.Services.AddSingleton<ContentHubFolderService>();
builder.Services.AddSingleton<MediaService>();
builder.Services.AddSingleton<AttachmentsService>();
builder.Services.AddSingleton<ContentItemService>();
builder.Services.AddSingleton<PageService>();
builder.Services.AddSingleton<WidgetService>();
builder.Services.AddSingleton<SectionService>();
builder.Services.AddSingleton<NavigationService>();

builder.Services.AddSingleton<MediaConverter>();
builder.Services.AddSingleton<AttachmentConverter>();
builder.Services.AddSingleton<FieldConverters>();

var assembly = Assembly.GetExecutingAssembly();

// Register content item converters
var contentItemConverterType = typeof(IContentItemConverter);
var contentItemConverterImplementations = assembly.GetTypes()
    .Where(t => contentItemConverterType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

foreach (var impl in contentItemConverterImplementations)
{
    builder.Services.AddSingleton(contentItemConverterType, impl);
}

// Register page converters
var pageConverterType = typeof(IPageConverter);
var pageConverterImplementations = assembly.GetTypes()
    .Where(t => pageConverterType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

foreach (var impl in pageConverterImplementations)
{
    builder.Services.AddSingleton(pageConverterType, impl);
}

// Register section converters
var sectionConverterType = typeof(ISectionConverter);
var sectionConverterImplementations = assembly.GetTypes()
    .Where(t => sectionConverterType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

foreach (var impl in sectionConverterImplementations)
{
    builder.Services.AddSingleton(sectionConverterType, impl);
}

// Register widget converters
var widgetConverterType = typeof(IWidgetConverter);
var widgetConverterImplementations = assembly.GetTypes()
    .Where(t => widgetConverterType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

foreach (var impl in widgetConverterImplementations)
{
    builder.Services.AddSingleton(widgetConverterType, impl);
}

var app = builder.Build();

CMS.Base.SystemContext.WebApplicationPhysicalPath = builder.Configuration.GetValue<string>("ApplicationRootPath");
CMSApplication.Init();

using (var scope = app.Services.CreateScope())
{
    var exportService = scope.ServiceProvider.GetService<ExportService>();
    await exportService.RunAsync();
}


