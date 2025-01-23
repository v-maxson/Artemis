using HtmlAgilityPack;

namespace NWDB;

public class NWDBClient
{
    private static class Urls
    {
        public const string BaseUrl = "https://nwdb.info";

        public static string ItemPageUrl(int page) => $"{BaseUrl}/db/items/page/{page}";
    }

    private readonly HtmlWeb HtmlWeb = new();

    public async Task<IEnumerable<Item>> GetItemsAsync(int page)
    {

        var doc = await HtmlWeb.LoadFromWebAsync(Urls.ItemPageUrl(page));

        // Get the items.
        var tableContents = doc.DocumentNode
            .QuerySelector("div.table-responsive") // Table containing the items.
            .QuerySelector("tbody.align-middle") // Table body containing the items.
            .QuerySelectorAll("tr"); // Rows containing the items.

        var items = new List<Item>();

        foreach (var docItem in tableContents)
        {
            var itemEllipsis = docItem
                .QuerySelector("td.ellipsis") // Item name box.
                .QuerySelector("a.table-item-name");
            var item = new Item()
            {
                Name = itemEllipsis.InnerText,
                Url = Urls.BaseUrl + itemEllipsis.GetAttributeValue("href", "")
            };

            items.Add(item);
        }

        return items;
    }
}
