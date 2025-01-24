using DB.Models;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord;

namespace Modules;

[SlashCommand(
    "settings", 
    "Commands for managing your server settings.", 
    Contexts = [InteractionContextType.Guild],
    DefaultGuildUserPermissions = Permissions.ManageGuild
)]
public partial class GuildSettingsModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private async Task HandleSettingAsync(string description, Action<GuildSettings> updateAction)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
        GuildSettings.Upsert(Context.Guild!.Id, updateAction);
        await ModifyResponseAsync(msg =>
        {
            msg.Embeds = [
                new EmbedProperties()
                    .WithDescription(description)
                    .WithColor(Colors.Green)
            ];
        });
    }

    #region Logs Metadata
    [SubSlashCommand("logchannel", "Set the channel for logs to be sent to.")]
    public partial Task LogChannelAsync(
        [SlashCommandParameter(Name = "channel", Description = "The channel to send logs to.")]
        TextGuildChannel channel
    );
    #endregion
    public async partial Task LogChannelAsync(TextGuildChannel channel)
    {
        await HandleSettingAsync(
            $"Logs will now be sent to <#{channel.Id}>.",
            gs => gs.LogsChannelId = channel.Id
        );
    }

    #region ModLogs Metadata
    [SubSlashCommand("modlogs", "Enable/Disable Moderation Logs.")]
    public partial Task ModLogsAsync(
        [SlashCommandParameter(Name = "enabled", Description = "True to enable moderation logs, false otherwise.")]
        bool enabled
    );
    #endregion
    public async partial Task ModLogsAsync(bool enabled)
    {
        await HandleSettingAsync(
            $"Moderation logs are now {(enabled ? "enabled" : "disabled")}.",
            gs => gs.ModerationLogsEnabled = enabled
        );
    }

    #region JoinLeaveLogs Metadata
    [SubSlashCommand("joinleavelogs", "Enable/Disable Join/Leave Logs.")]
    public partial Task JoinLeaveLogsAsync(
        [SlashCommandParameter(Name = "enabled", Description = "True to enable join/leave logs, false otherwise.")]
        bool enabled
    );
    #endregion
    public async partial Task JoinLeaveLogsAsync(bool enabled)
    {
        await HandleSettingAsync(
            $"Join/Leave logs are now {(enabled ? "enabled" : "disabled")}.",
            gs => gs.JoinLeaveLogsEnabled = enabled
        );
    }

    #region MessageEditLogs Metadata
    [SubSlashCommand("messageeditlogs", "Enable/Disable Message Edit Logs.")]
    public partial Task MessageEditLogsAsync(
        [SlashCommandParameter(Name = "enabled", Description = "True to enable message edit logs, false otherwise.")]
        bool enabled
    );
    #endregion
    public async partial Task MessageEditLogsAsync(bool enabled)
    {
        await HandleSettingAsync(
            $"Message edit logs are now {(enabled ? "enabled" : "disabled")}.",
            gs => gs.MessageEditLogsEnabled = enabled
        );
    }

    #region MessageDeleteLogs Metadata
    [SubSlashCommand("messagedeletelogs", "Enable/Disable Message Delete Logs.")]
    public partial Task MessageDeleteLogsAsync(
        [SlashCommandParameter(Name = "enabled", Description = "True to enable message delete logs, false otherwise.")]
        bool enabled
    );
    #endregion
    public async partial Task MessageDeleteLogsAsync(bool enabled)
    {
        await HandleSettingAsync(
            $"Message delete logs are now {(enabled ? "enabled" : "disabled")}.",
            gs => gs.MessageDeleteLogsEnabled = enabled
        );
    }

    #region AllLogs Metadata
    [SubSlashCommand("all_logs", "Enable/Disable all Logs.")]
    public partial Task AllLogsAsync(
        [SlashCommandParameter(Name = "enabled", Description = "True to enable all logs, false otherwise.")]
        bool enabled
    );
    #endregion
    public async partial Task AllLogsAsync(bool enabled)
    {
        await HandleSettingAsync(
            $"ALL logs are now {(enabled ? "enabled" : "disabled")}.",
            gs =>
            {
                gs.ModerationLogsEnabled = enabled;
                gs.JoinLeaveLogsEnabled = enabled;
                gs.MessageEditLogsEnabled = enabled;
                gs.MessageDeleteLogsEnabled = enabled;
            }
        );
    }

    #region AutoRole Metadata
    [SubSlashCommand("autorole", "Set the role to be assigned to new members.")]
    public partial Task AutoRoleAsync(
        [SlashCommandParameter(Name = "role", Description = "The role to assign to new members.")]
        Role role
    );
    #endregion
    public async partial Task AutoRoleAsync(Role role)
    {
        await HandleSettingAsync(
            $"Auto Role has been set to {role}.",
            gs => gs.AutoRoleId = role.Id
        );
    }
}
