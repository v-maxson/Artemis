
using NetCord;
using NetCord.Gateway;

internal static class VoiceGuildChannelExtensions
{
    public static async Task<int?> GetConnectedUserCountAsync(this VoiceGuildChannel channel, GatewayClient client)
    {
        // Get the Guild this voice channel belongs to.
        var guild = await client.GetOrFetchGuildAsync(channel.GuildId); if (guild == null) return null;

        var guildVoiceStates = guild.VoiceStates.Where(x => x.Value.ChannelId == channel.Id);
        return guildVoiceStates.Count();
    }
}

