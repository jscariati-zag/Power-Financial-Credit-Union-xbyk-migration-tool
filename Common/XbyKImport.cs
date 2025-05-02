using System.Text.Json.Serialization;

namespace Common
{
    public class XbyKImport
    {
        public List<ContentHubFolder> ContentHubFolders { get; set; } = new List<ContentHubFolder>();
        public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
        public List<Page> Pages { get; set; } = new List<Page>();
    }

    public class Page
    {
        public Guid OldGuid { get; set; }
        public int WebPageItemID { get; set; }
        public Guid WebPageItemGUID { get; set; }
        public Guid ContentItemGUID { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        public bool Published { get; set; }
        public string ContentType { get; set; }
        public string DisplayName { get; set; }
        public string Language { get; set; }
        public string UrlSlug { get; set; }
        public Dictionary<string, object> ItemData { get; set; }
        public WidgetConfiguration WidgetConfiguration { get; set; }
        public TemplateConfiguration TemplateConfiguration { get; set; }
        public Page Parent { get; set; }
        public List<Page> Children { get; set; }
    }

    public class WidgetConfiguration
    {
        public List<EditableArea> editableAreas { get; set; }
    }

    public class EditableArea
    {
        public string identifier { get; set; }
        public List<Section> sections { get; set; }
    }

    public class Section
    {
        public string identifier { set; get; }
        public string type { set; get; }
        public Dictionary<string, object> properties { get; set; }
        public List<Zone> zones { get; set; }
        public Dictionary<string, object> fieldIdentifiers { get; set; }
    }

    public class Zone
    {
        public string identifier { set; get; }
        public string name { set; get; }
        public List<Widget> widgets { get; set; } = new List<Widget>();
    }

    public class Widget
    {
        public string identifier { set; get; }
        public string type { set; get; }
        public List<Variant> variants { get; set; }
    }

    public class Variant
    {
        public string identifier { set; get; }
        public Dictionary<string, object> properties { get; set; }
        public Dictionary<string, object> fieldIdentifiers { get; set; }
    }

    public class TemplateConfiguration
    {
        public string identifier { get; set; }
        public Dictionary<string, object> properties { get; set; }
        public Dictionary<string, object> fieldIdentifiers { get; set; }
    }

    public class ContentItem
    {
        public Guid OldGuid { get; set; }
        public int ContentItemID { get; set; }
        public Guid ContentItemGUID { get; set; }
        public string ContentType { get; set; }
        public string DisplayName { get; set; }
        public string Language { get; set; }
        public string FolderName { get; set; }
        public bool Published { get; set; }
        public Dictionary<string, object> ItemData { get; set; }
    }

    public class ContentHubFolder
    {
        public int ContentFolderID { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
    }

    public class Asset
    {
        public string AssetUrl { get; set; }
        public Guid FileGuid { get; set; }
    }

    public class ContentReference
    {
        public Guid OldGuid { get; set; }
    }

    public class PageReference
    {
        public Guid OldGuid { get; set; }
    }
}
