using HtmlAgilityPack;
using LiteDB;

namespace NWDB;

public class Entity
{
    // Item should only be constructed internally.
    internal Entity() { }

    [BsonId]
    internal int Id { get; set; }

    /// <summary>
    /// The nwdb.info URL for the item.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public required string Name { get; set; }
}
