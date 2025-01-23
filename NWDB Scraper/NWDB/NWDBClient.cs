using HtmlAgilityPack;
using LiteDB;

namespace NWDB;

public class NWDBClient
{
    internal static class Constants
    {
        public const string BaseUrl = "https://nwdb.info";

        public static string PageUrl(string collection, int page) => $"{BaseUrl}/db/{collection}/page/{page}?sort=name_asc";
    }

    private readonly HtmlWeb HtmlWeb = new();

    public async Task<IEnumerable<Entity>> GetEntitiesAsync(string url)
    {

        var doc = await HtmlWeb.LoadFromWebAsync(url);

        // Get the items.
        var tableContents = doc.DocumentNode
            .QuerySelector("div.table-responsive") // Table containing the items.
            .QuerySelector("tbody.align-middle") // Table body containing the items.
            .QuerySelectorAll("tr"); // Rows containing the items.

        var items = new List<Entity>();

        foreach (var docItem in tableContents)
        {
            var itemEllipsis = docItem
                .QuerySelector("td.ellipsis") // Item name box.
                .QuerySelector("a.table-item-name");
            var item = new Entity()
            {
                Name = itemEllipsis.InnerText,
                Url = Constants.BaseUrl + itemEllipsis.GetAttributeValue("href", "")
            };

            items.Add(item);
        }

        return items;
    }

    internal async Task AddEntitiesToDb(LiteDatabase db, string collectionName, int pageCount)
    {
        /// "status-effects" is a special case.
        ILiteCollection<Entity> items;
        if (collectionName == "status-effects") items = db.GetCollection<Entity>("statuseffects");
        else items = db.GetCollection<Entity>(collectionName);

        for (int i = 1; i <= pageCount; i++)
        {
            var pageItems = await GetEntitiesAsync(Constants.PageUrl(collectionName, i));
            items.InsertBulk(pageItems);
            Console.WriteLine("Retrieved {0} page {1}/{2}", collectionName, i, pageCount);
            if (i % 10 == 0) await Task.Delay(10);
        }
    }
}
