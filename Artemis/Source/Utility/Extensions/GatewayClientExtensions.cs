
using NetCord.Gateway;

internal static class GatewayClientExtensions
{
    /// <summary>
    /// Attempts to retreive the Guild from the Cache. If it doesn't exist, it will attempt to fetch it from the REST API.
    /// </summary>
    public static async Task<Guild?> GetOrFetchGuildAsync(this GatewayClient client, ulong guildId) {
        client.Cache.Guilds.TryGetValue(guildId, out var guild);
        if (guild == null) guild = (Guild?)await client.Rest.GetGuildAsync(guildId);
        return guild;
    }

    public static bool TryGetGuild(this GatewayClient client, ulong guildId, out Guild? guild) {
        return client.Cache.Guilds.TryGetValue(guildId, out guild);
    }
}
