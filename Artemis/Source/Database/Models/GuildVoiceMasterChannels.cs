namespace DB.Models;

internal class GuildVoiceMasterChannels : DatabaseModel<GuildVoiceMasterChannels>
{
    /// <summary>
    /// Key: Channel Id | Value: Owner User Id
    /// </summary>
    public Dictionary<ulong, ulong> Channels { get; set; } = [];
}
