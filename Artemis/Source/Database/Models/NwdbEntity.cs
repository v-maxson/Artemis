namespace Artemis.DB.Models;

public class NwdbEntity : DatabaseModel<NwdbEntity>
{
    /// <summary>
    /// The nwdb.info URL for the item.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public required string Name { get; set; }
}
