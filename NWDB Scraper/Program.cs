// This is a utility to add items to a "nwdb.db" LiteDB database.
// Should only need to be run once to collect all of the data.

using LiteDB;
using NWDB;

Console.WriteLine("Collecting data...");


var client = new NWDBClient();
using var db = new LiteDatabase("nwdb.db");

const int ItemsPageCount = 693;
const int MountsPageCount = 2;
const int RecipesPageCount = 158;
const int AbilitiesPageCount = 12;
const int PerksPageCount = 23;
const int StatusEffectsPageCount = 67;
const int QuestsPageCount = 95;
const int CreaturesPageCount = 150;
const int GatherablesPageCount = 81;
const int ShopsPageCount = 22;
const int NPCSPageCount = 25;
const int ZonesPageCount = 26;

var stopwatch = System.Diagnostics.Stopwatch.StartNew();

// Batch these to avoid rate limit.
await Task.WhenAll(
    client.AddEntitiesToDb(db, "items", ItemsPageCount),
    client.AddEntitiesToDb(db, "mounts", MountsPageCount)
);

await Task.WhenAll(
    client.AddEntitiesToDb(db, "recipes", RecipesPageCount),
    client.AddEntitiesToDb(db, "abilities", AbilitiesPageCount)
);

await Task.WhenAll(
    client.AddEntitiesToDb(db, "perks", PerksPageCount),
    client.AddEntitiesToDb(db, "status-effects", StatusEffectsPageCount)
);

await Task.WhenAll(
    client.AddEntitiesToDb(db, "quests", QuestsPageCount),
    client.AddEntitiesToDb(db, "creatures", CreaturesPageCount)
);

await Task.WhenAll(
    client.AddEntitiesToDb(db, "gatherables", GatherablesPageCount),
    client.AddEntitiesToDb(db, "shops", ShopsPageCount)
);

await Task.WhenAll(
    client.AddEntitiesToDb(db, "npcs", NPCSPageCount),
    client.AddEntitiesToDb(db, "zones", ZonesPageCount)
);

stopwatch.Stop();
Console.WriteLine($"Total data collection took {stopwatch.Elapsed}");
