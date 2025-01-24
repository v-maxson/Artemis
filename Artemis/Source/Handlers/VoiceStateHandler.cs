using DB.Models;
using Modules;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using Serilog;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.VoiceStateUpdate))]
public class VoiceStateHandler(GatewayClient client) : IGatewayEventHandler<VoiceState>
{
    private readonly GatewayClient Client = client;

    public async ValueTask HandleAsync(VoiceState currentState)
    {
        if (currentState.User!.IsBot) return;
        var previousState = GetPreviousStateAsync(currentState);
        if (currentState != null) await CreateVoiceMasterChannel(currentState);
        if (previousState != null) await DeleteVoiceMasterChannel(previousState);
    }

    private VoiceState? GetPreviousStateAsync(VoiceState state)
    {
        Client.TryGetGuild(state.GuildId, out var guild);
        if (guild == null) return null;

        guild.VoiceStates.TryGetValue(state.UserId, out var previousState);
        return previousState;
    }

    private async Task CreateVoiceMasterChannel(VoiceState currentState)
    {
        // Get the settings for the current guild
        // If no settings are found or the current channel is not the VoiceMaster channel, return.
        if (!GuildSettings.TryGet(currentState.GuildId, out var guildSettings)) return;
        if (guildSettings.VoiceMasterChannelId != currentState.ChannelId) return;

        // Fetch the guild information from the client
        // If the guild is not found, log an error and exit the method
        var guild = await Client.GetOrFetchGuildAsync(currentState.GuildId);

        if (guild == null)
        {
            Log.Error("Failed to create VoiceMaster channel for guild {GuildId}. Guild not found.", currentState.GuildId);
            return;
        }


        // Get the VoiceMaster settings for the current user, or create default settings if none exist
        if (!UserVoiceMasterSettings.TryGet(currentState.UserId, out var userSettings)) 
            userSettings =  UserVoiceMasterSettings.Default(currentState.User!);

        // Update (or insert) the user's settings in the database.
        UserVoiceMasterSettings.Upsert(userSettings);

        // Get the current voice channel the user is in.
        var currentChannel = (VoiceGuildChannel)guild.Channels[(ulong)currentState.ChannelId!];

        // Create a new voice channel with the user's settings.
        var createdChannel = await guild.CreateChannelAsync(
            new GuildChannelProperties(userSettings.ChannelName, ChannelType.VoiceGuildChannel)
                .WithUserLimit(userSettings.ChannelLimit)
                .WithPermissionOverwrites([
                    new PermissionOverwriteProperties(currentState.UserId, PermissionOverwriteType.User)
                        .WithAllowed(Permissions.ManageChannels)
                ])
                .WithParentId(currentChannel.ParentId)
        );

        var guildVmChannels = GuildVoiceMasterChannels.GetOrCreate(currentState.GuildId);

        // Add the newly created channel to the guild's VoiceMaster channels.
        guildVmChannels.Channels[createdChannel.Id] = currentState.UserId;

        // Update the collection with the new channel information.
        GuildVoiceMasterChannels.Upsert(guildVmChannels);

        // Move the user into the newly created channel.
        await currentState.User!.ModifyAsync(x => x.ChannelId = createdChannel.Id);

        // Send the settings embed + components to the newly created channel.
        await ((TextChannel)createdChannel).SendMessageAsync(
            new MessageProperties()
            .WithContent($"<@{currentState.User!.Id}>")
            .WithEmbeds([VoiceMasterSettingsModule.SettingsEmbed])
            .WithComponents([VoiceMasterStringMenuModule.SettingsMenu])
            );
    }

    private async Task DeleteVoiceMasterChannel(VoiceState previousState)
    {
        if (!GuildVoiceMasterChannels.TryGet(previousState.GuildId, out var guildVmChannels)) return;
        if (!guildVmChannels.Channels.ContainsKey((ulong)previousState.ChannelId!)) return;

        // Retrieve the guild and its channels.
        var guild = await Client.Rest.GetGuildAsync(previousState.GuildId);
        var voiceChannel = (await guild.GetChannelsAsync()).OfType<VoiceGuildChannel>().FirstOrDefault(x => x.Id == previousState.ChannelId);
        if (voiceChannel == null) return;

        // Check if the previous channel is empty, if it is, delete it.
        if (await voiceChannel.GetConnectedUserCountAsync(Client) == 0)
        {
            await voiceChannel.DeleteAsync();
            guildVmChannels.Channels.Remove((ulong)previousState.ChannelId!);
            GuildVoiceMasterChannels.Upsert(guildVmChannels);
        }
    }
}
