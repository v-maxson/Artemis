
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Database.Models;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.GuildChannelDelete))]
public class VoiceChannelDeleteHandler : IGatewayEventHandler<IGuildChannel>
{
    public ValueTask HandleAsync(IGuildChannel channel)
    {
        // If the channel is not a voice channel, return.
        if (channel is not VoiceGuildChannel voiceChannel) return default;

        using var db = Database.Database.Connect();
        var channelsCollection = GuildVoiceMasterChannels.GetCollection(db);
        var guildVmChannels = GuildVoiceMasterChannels.GetInCollection(voiceChannel.GuildId, channelsCollection);
        if (guildVmChannels is null) return default;

        // If guildVmChannels includes the channel, delete it from the collection.
        if (guildVmChannels.Channels.TryGetValue(voiceChannel.Id, out ulong channelOwnerId))
        {
            guildVmChannels.Channels.Remove(voiceChannel.Id);
            channelsCollection.Update(guildVmChannels);
        }

        return default;
    }
}
