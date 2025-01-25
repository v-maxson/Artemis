namespace Artemis.DB.Models;

public class NwdbEntity
{
    [LiteDB.BsonId]
    public int Id { get; set; }

    /// <summary>
    /// The nwdb.info URL for the item.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public required string Name { get; set; }
}
