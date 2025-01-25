
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Artemis.DB.Models;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.GuildChannelUpdate))]
public class VoiceChannelUpdateHandler : IGatewayEventHandler<IGuildChannel>
{
    public ValueTask HandleAsync(IGuildChannel channel) {
        // If the channel is not a voice channel, return.
        if (channel is not VoiceGuildChannel voiceChannel) return default;

        // If guildVmChannels doesn't include the channel, return.
        if (!GuildVoiceMasterChannels.TryGet(voiceChannel.GuildId, out var guildVmChannels)) return default;
        if (!guildVmChannels.Channels.TryGetValue(voiceChannel.Id, out ulong channelOwnerId)) return default;
        if (!UserVoiceMasterSettings.TryGet(channelOwnerId, out var userVmSettings)) return default; // It should never be null, but just in case.

        // Check for applicable settings to change and update them in the user's settings.
        bool updated = false;
        if (voiceChannel.Name != userVmSettings.ChannelName) {
            userVmSettings.ChannelName = voiceChannel.Name;
            updated = true;
        }
        if (voiceChannel.UserLimit != userVmSettings.ChannelLimit) {
            userVmSettings.ChannelLimit = voiceChannel.UserLimit;
            updated = true;
        }

        // If any settings were changed, update the DB.
        if (updated) UserVoiceMasterSettings.Upsert(userVmSettings);

        return default;
    }
}
