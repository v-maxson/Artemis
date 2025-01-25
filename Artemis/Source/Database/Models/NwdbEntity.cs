namespace Artemis.DB.Models;

public class NwdbEntity : DatabaseModel<NwdbEntity>
{
    [LiteDB.BsonId]
    public new int Id { get; set; }

    /// <summary>
    /// The nwdb.info URL for the item.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public required string Name { get; set; }
}
