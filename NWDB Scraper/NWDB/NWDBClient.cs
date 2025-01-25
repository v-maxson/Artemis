using HtmlAgilityPack;
using LiteDB;
using Artemis.DB.Models;

namespace NWDB;

public class NWDBClient
{
    internal static class Constants
    {
        public const string BaseUrl = "https://nwdb.info";

        public static string PageUrl(string collection, int page) => $"{BaseUrl}/db/{collection}/page/{page}?sort=name_asc";
    }

    private readonly HtmlWeb HtmlWeb = new();
    private readonly Random Random = new();

    public NWDBClient() {
        // Randomize User-Agent to avoid rate limit.
        HtmlWeb.PreRequest += (request) => {
            // Thanks ChatGPT
            string[] headers = [
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; rv:87.0) Gecko/20100101 Firefox/87.0",
                "Mozilla/5.0 (Linux; Android 10; Pixel 3 XL Build/QP1A.191105.003) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Mobile Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/92.0.902.67 Safari/537.36",
                "Mozilla/5.0 (Linux; U; Android 11; en-US; SM-G991U Build/RQ3A.210905.001) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.61 Mobile Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_6) AppleWebKit/537.36 (KHTML, like Gecko) Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Safari/537.36 Edg/94.0.992.31",
                "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:89.0) Gecko/20100101 Firefox/89.0",
                "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:62.0) Gecko/20100101 Firefox/62.0"
                ];

            request.Headers.Add("User-Agent", headers[Random.Next(0, headers.Length)]);
            return true;
        };
    }

    public async Task<IEnumerable<NwdbEntity>> GetEntitiesAsync(string url) {

        var doc = await HtmlWeb.LoadFromWebAsync(url);

        // Get the items.
        var tableContents = doc.DocumentNode
            .QuerySelector("div.table-responsive") // Table containing the items.
            .QuerySelector("tbody.align-middle") // Table body containing the items.
            .QuerySelectorAll("tr"); // Rows containing the items.

        var items = new List<NwdbEntity>();

        foreach (var docItem in tableContents) {
            var itemEllipsis = docItem
                .QuerySelector("td.ellipsis") // Item name box.
                .QuerySelector("a.table-item-name");
            var item = new NwdbEntity() {
                Name = itemEllipsis.InnerText,
                Url = Constants.BaseUrl + itemEllipsis.GetAttributeValue("href", "")
            };

            items.Add(item);
        }

        return items;
    }

    internal async Task AddEntitiesToDb(LiteDatabase db, string collectionName, int pageCount) {
        /// "status-effects" is a special case.
        ILiteCollection<NwdbEntity> items;
        if (collectionName == "status-effects") items = db.GetCollection<NwdbEntity>("statuseffects");
        else items = db.GetCollection<NwdbEntity>(collectionName);

        for (int i = 1; i <= pageCount; i++) {
            var pageItems = await GetEntitiesAsync(Constants.PageUrl(collectionName, i));
            items.InsertBulk(pageItems);
            Console.WriteLine("Retrieved {0} page {1}/{2}", collectionName, i, pageCount);

            // Wait a little bit to avoid rate limit.
            if (i % 10 == 0) await Task.Delay(1000);
        }
    }
}
