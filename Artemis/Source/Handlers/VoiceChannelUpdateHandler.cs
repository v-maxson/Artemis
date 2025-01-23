
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Database.Models;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.GuildChannelUpdate))]
public class VoiceChannelUpdateHandler : IGatewayEventHandler<IGuildChannel>
{
    public ValueTask HandleAsync(IGuildChannel channel)
    {
        // If the channel is not a voice channel, return.
        if (channel is not VoiceGuildChannel voiceChannel) return default;

        using var db = Database.Database.Connect();
        var guildVmChannels = GuildVoiceMasterChannels.GetInCollection(voiceChannel.GuildId, GuildVoiceMasterChannels.GetCollection(db))?.Channels;
        if (guildVmChannels is null) return default;

        // If guildVmChannels doesn't include the channel, return.
        if (!guildVmChannels.TryGetValue(voiceChannel.Id, out ulong channelOwnerId)) return default;
        var settingsCollection = UserVoiceMasterSettings.GetCollection(db);
        var userVmSettings = UserVoiceMasterSettings.GetInCollection(channelOwnerId, settingsCollection);
        if (userVmSettings is null) return default; // It should never be null, but just in case.

        // Check for applicable settings to change and update them in the user's settings.
        bool updated = false;
        if (voiceChannel.Name != userVmSettings.ChannelName)
        {
            userVmSettings.ChannelName = voiceChannel.Name;
            updated = true;
        }
        if (voiceChannel.UserLimit != userVmSettings.ChannelLimit)
        {
            userVmSettings.ChannelLimit = voiceChannel.UserLimit;
            updated = true;
        }

        // If any settings were changed, update the DB.
        if (updated) settingsCollection.Update(userVmSettings);

        return default;
    }
}