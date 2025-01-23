// This is a utility to add items to a "nwdb.db" LiteDB database.
// Should only need to be run once to collect all of the data.

using LiteDB;
using NWDB;

const int ITEM_PAGE_COUNT = 693;

Console.WriteLine("Collecting items...");
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

var client = new NWDBClient();

using var db = new LiteDatabase("nwdb.db");
var items = db.GetCollection<Item>("items");

for (int i = 1; i <= ITEM_PAGE_COUNT; i++)
{
    var pageItems = await client.GetItemsAsync(i);
    items.InsertBulk(pageItems);
    Console.WriteLine("Retrieved Items page {0}/{1}", i, ITEM_PAGE_COUNT);

    if (i % 10 == 0)
    {
        Console.WriteLine("Waiting 5 seconds...");
        await Task.Delay(5000);
    }
}

stopwatch.Stop();
Console.WriteLine($"Collected {items.Count()} items in {stopwatch.ElapsedMilliseconds}ms.");
