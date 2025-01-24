
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using DB.Models;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.GuildChannelDelete))]
public class VoiceChannelDeleteHandler : IGatewayEventHandler<IGuildChannel>
{
    public ValueTask HandleAsync(IGuildChannel channel)
    {
        // If the channel is not a voice channel, return.
        if (channel is not VoiceGuildChannel voiceChannel) return default;

        
        if (!GuildVoiceMasterChannels.TryGet(voiceChannel.GuildId, out var guildVmChannels)) return default;

        // If guildVmChannels includes the channel, delete it from the collection.
        if (guildVmChannels.Channels.TryGetValue(voiceChannel.Id, out _))
        {
            guildVmChannels.Channels.Remove(voiceChannel.Id);
            GuildVoiceMasterChannels.Upsert(guildVmChannels);
        }

        return default;
    }
}
