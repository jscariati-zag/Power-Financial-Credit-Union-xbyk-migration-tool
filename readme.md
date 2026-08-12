# Power Financial Credit Union Migration Tool
The original migration tool is intended to migrate the entire site. This project includes changes to accommodate only importing blog content into an existing XbyK site

The export tool has two apps. Each of which function through the console without UI. One app handles export of the K13 site/content into a json file. The other app handles import of the json file into the XbyK site. Each is run in the IDE, by targeting the project and running with the green play button (with or without debug) as you would start a site. As each runs, the console should fill out nicely with detail as to what's going right or wrong. Given the separation in projects, there is a natural break in the process where the export stops, and you have a json file that you can look through and familiarize yourself with to troubleshoot as necessary.

## Export
 * \ZAGK13Export\app.config should use the K13 connection string
 * \ZAGK13Export\appsettings.json - these fields should be updated, in a way that is hopefully self-evident. "ComponentContainerType" should be the page type name of the folder under which K13 widgets are added. This name may change from site-to-site, and the export typically needs to know what it's called. Granted, in my case the widget import functionality was entirely unused, as only blog content was imported.
 * \ZAGK13Export\Services\ExportService.cs is the start of the export, where the individual export services are called in turn.

### ConvertMediaLibraryFolders:
We start with media exports. In my case, I've added logic in MediaService.cs to check the library folder name to filter out the large number of images on the site that are unrelated to blog content.

### ConvertMediaFiles:
There was also a need to specifically assign ContentItemID in MediaConverter.cs. Without this, images were not functioning properly (in some way I'v forgotten)

### ConvertAttachments & AddContentHubFolders:
I don't honestly recall whether or not ConvertAttachments() and AddContentHubFolders() were run or left commented out. In either case, neither function was had custom edits for the blog import purposes, so just uncomment them if it seems to be needed

### ConvertContentItems:
was run, and had no custom changes

### ConvertPages:
Rather than starting at the main blog page directly, I ran this on the whole site. The function itself doesn't filter out pages that are not blog-related, but this is handled by the available converter files under /Converters/Pages
Notable, I commented out every not blog-related page converter (I'm sure they could have been deleted) and edited/created converter files for the page types that need to be converted - ensuring that each page field we need is included and is assigned with a string to be used in the import process. Even if we're not importing the main blog or blog category pages, I found they do need to be exported so the traversal can find all the necessary posts.

### AddNavigation
was not run, as blog posts aren't included in the nav


## Json file
Json file should get output wherever you've specified in appsettings.json. When troubleshooting export especially, it may be useful to look through this file in a text editor and assess things yourself.

In my case, there were some manual adjustments I had to make to this json file before import, though I expect that is atypical. There were two posts with a name collision that K13 didn't care about, but would throw an error when importing to XbyK. I simply added a "-1" to the name in this json file before running the import.

## Import
 * \ZAGXbyKImport\appsettings.json this should be updated point to the XbyK site. Most fields are likely fine as-is
 * \ZAGXbyKImport\Services\ImportService.cs is the start of the import, where import services are called in turn.

### Delete Services
The delete services are commented out. If we were importing an entire site, we'd want to start fresh. In this case, we don't want to delete anything. Whenever an import goes wrong, I would just manually delete whatever had been imported (the select blog pages and media content items) and run again once I thought the issue was fixed. Or worst case sceneario, run a DB restore of a last-known-good.

### AddContentHubFolders
There is some custom logic in GetIDFromName to name folders that aren't found with the default XbyK import. I don't know whether this is generally applicable to sites or specific to this site

### AddContentItems
Runs without adjustment

### AddPages
The PageService has a lot of commented code that I sloppily left in. This is predominantly lines added to troubleshoot during some bug or other, and is rightly commented out. The one true addition here is in UpdatePageReference where we check the page ContentType to only import BlogPosts.

### UpdateContentItemReferences
Runs without adjustment

### UpdatePageReferences
Runs without adjustment


## Notes:
I did not successfully import blog post categories from the old site to the new site. If this is crucial for your project, you must pick up where I left off; I can take you no futher.

Export/import can take a little while depending on how much content you're dealing with. In my case, I didn't particularly bother importing the main blog or life-stage (blog category) pages perfectly. There are only a handful, and it saved me time just to manually update those at the end rather than wrangle the migration tool. Similarly, I just imported things to the root of the site, or wherever, and manually repositioned things as needed.

