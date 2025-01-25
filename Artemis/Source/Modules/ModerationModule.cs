using Artemis.DB.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace Modules;

[SlashCommand(
    "moderation",
    "Commands for moderating the server.",
    Contexts = [InteractionContextType.Guild],
    DefaultGuildUserPermissions = Permissions.Administrator
)]
public partial class ModerationHistoryModule : ApplicationCommandModule<ApplicationCommandContext>
{
    #region History Metadata
    [SubSlashCommand("history", "Retrieve detailed moderation history for a user.")]
    public partial Task HistoryAsync(
        [SlashCommandParameter(Name = "user", Description = "The user to retrieve the moderation history for.")]
        GuildUser user,

        [SlashCommandParameter(Name = "page", Description = "The page of the moderation history to retrieve.")]
        int page = 1
    );
    #endregion
    public async partial Task HistoryAsync(GuildUser user, int page) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var guildModeration = GuildModeration.GetOrCreate(Context.Guild!.Id);
        if (!guildModeration.ModeratedUsers.TryGetValue(user.Id, out var infractions) || infractions.Count == 0) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        description: $"<@{user.Id}> has no moderation history!",
                        color: Colors.Green
                    )
                ];
            });
            return;
        }

        var infractionsChunks = infractions.AsEnumerable().Chunk(5).ToList();
        int validPage = Math.Clamp(page, 1, infractionsChunks.Count);

        var infractionChunk = infractionsChunks[validPage - 1];

        List<EmbedFieldProperties> embedFields = [];

        foreach (var infraction in infractionChunk) {
            embedFields.Add(
                EmbedHelper.Field(
                    $"{infraction.InfractionType}",
                    $" - **Reason**: {infraction.Reason}\n - **Moderator**: <@{infraction.ModeratorId}>\n - **Occured**: <t:{((DateTimeOffset)infraction.Timestamp).ToUnixTimeSeconds()}>"
                )
            );
        }

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    title: $"Moderation History for {user.Nickname}",
                    fields: embedFields,
                    color: Colors.Blue,
                    thumbnail: new EmbedThumbnailProperties(user.GetAvatarUrl()?.ToString()),
                    footer: EmbedHelper.Footer($"Page {validPage}/{infractionsChunks.Count}")
                )
            ];
        });
    }
}

public partial class ModerationModule : ApplicationCommandModule<ApplicationCommandContext>
{


    #region Warn Metadata
    [SlashCommand("warn", "Warns a user.", Contexts = [InteractionContextType.Guild], DefaultGuildUserPermissions = Permissions.ModerateUsers)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.ModerateUsers)]
    public partial Task WarnAsync(
        [SlashCommandParameter(Name = "user", Description = "The user to warn.")]
        GuildUser user,

        [SlashCommandParameter(Name = "reason", Description = "The reason for warning this user.")]
        string reason,

        [SlashCommandParameter(Name = "silent", Description = "If true, does not broadcast this action to the server.")]
        bool silent = false,

        [SlashCommandParameter(Name = "dm", Description = "If true, sends the user a DM.")]
        bool dm = false
    );
    #endregion
    public async partial Task WarnAsync(GuildUser user, string reason, bool silent, bool dm) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        AddInfraction(user.Id, GuildModeration.Infraction.Type.Warning, reason);

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    description: $"Done!",
                    color: Colors.Green
                )
            ];
        });
        if (!silent) {
            await FollowupAsync(
                new InteractionMessageProperties()
                .WithContent($"<@{user.Id}>")
                .WithEmbeds([
                    EmbedHelper.Embed(
                        description: $"<@{user.Id}> has been warned for **{reason}**.",
                        color: Colors.Orange
                    )
                ])
            );
        }
        if (dm) {
            await (await user.GetDMChannelAsync()).SendMessageAsync(new MessageProperties().WithEmbeds([
                EmbedHelper.Embed(
                    description: $"You have been warned in **{Context.Guild!.Name}** for **{reason}**.",
                    color: Colors.Orange
                )
            ]));
        }
    }

    #region Timeout Metadata
    public enum TimeoutDuration
    {
        [SlashCommandChoice("60 seconds")] SixtySeconds = 60,
        [SlashCommandChoice("5 minutes")] FiveMinutes = 300,
        [SlashCommandChoice("10 minutes")] TenMinutes = 600,
        [SlashCommandChoice("1 hour")] OneHour = 3600,
        [SlashCommandChoice("1 day")] OneDay = 86400,
        [SlashCommandChoice("1 week")] OneWeek = 604800
    }

    private static string GetDurationString(TimeoutDuration duration) {
        return duration switch {
            TimeoutDuration.SixtySeconds => "60 seconds",
            TimeoutDuration.FiveMinutes => "5 minutes",
            TimeoutDuration.TenMinutes => "10 minutes",
            TimeoutDuration.OneHour => "1 hour",
            TimeoutDuration.OneDay => "1 day",
            TimeoutDuration.OneWeek => "1 week",
            _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
        };
    }

    [SlashCommand("timeout", "Timeout a user.", Contexts = [InteractionContextType.Guild], DefaultGuildUserPermissions = Permissions.ModerateUsers)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.ModerateUsers)]
    public partial Task TimeoutAsync(
        [SlashCommandParameter(Name = "user", Description = "The user to timeout.")]
        GuildUser user,

        [SlashCommandParameter(Name = "duration", Description = "The duration of the timeout.")]
        TimeoutDuration duration,

        [SlashCommandParameter(Name = "reason", Description = "The reason for timing out this user.")]
        string reason,

        [SlashCommandParameter(Name = "silent", Description = "If true, does not broadcast this action to the server.")]
        bool silent = false,

        [SlashCommandParameter(Name = "dm", Description = "If true, sends the user a DM.")]
        bool dm = false
    );
    #endregion
    public async partial Task TimeoutAsync(GuildUser user, TimeoutDuration duration, string reason, bool silent, bool dm) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        try {
            await user.TimeOutAsync(DateTimeOffset.UtcNow.AddSeconds((int)duration));
        }
        catch (Exception e) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        description: $"Failed to timeout <@{user.Id}>: {e.Message}",
                        color: Colors.Red
                    )
                ];
            });
            return;
        }

        AddInfraction(user.Id, GuildModeration.Infraction.Type.Timeout, reason);

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    description: $"Done!",
                    color: Colors.Green
                )
            ];
        });

        if (!silent) {
            await FollowupAsync(
                new InteractionMessageProperties()
                .WithContent($"<@{user.Id}>")
                .WithEmbeds([
                    EmbedHelper.Embed(
                        description: $"You have been timed out for ***{GetDurationString(duration)}*** for **{reason}**.",
                        color: Colors.Orange
                    )
                ])
            );
        }

        if (dm) {
            await (await user.GetDMChannelAsync()).SendMessageAsync(new MessageProperties().WithEmbeds([
                EmbedHelper.Embed(
                    description: $"You have been timed out in **{Context.Guild!.Name}** for ***{GetDurationString(duration)}*** for **{reason}**.",
                    color: Colors.Orange
                )
            ]));
        }
    }

    #region Kick Metadata
    [SlashCommand("kick", "Kicks a user.", Contexts = [InteractionContextType.Guild], DefaultGuildUserPermissions = Permissions.KickUsers)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.KickUsers)]
    public partial Task KickAsync(
        [SlashCommandParameter(Name = "user", Description = "The user to kick.")]
        GuildUser user,

        [SlashCommandParameter(Name = "reason", Description = "The reason for kicking this user.")]
        string reason,

        [SlashCommandParameter(Name = "silent", Description = "If true, does not broadcast this action to the server.")]
        bool silent = false,

        [SlashCommandParameter(Name = "dm", Description = "If true, sends the user a DM.")]
        bool dm = false
    );
    #endregion
    public async partial Task KickAsync(GuildUser user, string reason, bool silent, bool dm) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        if (dm) {
            await (await user.GetDMChannelAsync()).SendMessageAsync(new MessageProperties().WithEmbeds([
                EmbedHelper.Embed(
                    description: $"You have been kicked from **{Context.Guild!.Name}** for **{reason}**.",
                    color: Colors.Orange
                )
            ]));
        }

        try {
            await user.KickAsync(new RestRequestProperties().WithAuditLogReason($"Kicked by {Context.User.Username} for: {reason}"));
        }
        catch (Exception e) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        description: $"Failed to kick <@{user.Id}>: {e.Message}",
                        color: Colors.Red
                    )
                ];
            });
            return;
        }

        AddInfraction(user.Id, GuildModeration.Infraction.Type.Kick, reason);

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    description: $"Done!",
                    color: Colors.Green
                )
            ];
        });

        if (!silent) {
            await FollowupAsync(
                new InteractionMessageProperties()
                .WithEmbeds([
                    EmbedHelper.Embed(
                        description: $"<@{user.Id}> has been kicked for **{reason}**.",
                        color: Colors.Orange
                    )
                ])
            );
        }
    }

    #region Ban Metadata
    public enum DeleteMessages
    {
        [SlashCommandChoice("Don't Delete Any")] None = 0,
        [SlashCommandChoice("Previous Hour")] PreviousHour = 3600,
        [SlashCommandChoice("Previous 6 Hours")] PreviousSixHours = 21600,
        [SlashCommandChoice("Previous 12 Hours")] PreviousTwelveHours = 43200,
        [SlashCommandChoice("Previous 24 Hours")] PreviousDay = 86400,
        [SlashCommandChoice("Previous 3 Days")] PreviousThreeDays = 259200,
        [SlashCommandChoice("Previous 7 Days")] PreviousWeek = 604800,
    }

    [SlashCommand("ban", "Bans a user.", Contexts = [InteractionContextType.Guild], DefaultGuildUserPermissions = Permissions.BanUsers)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.BanUsers)]
    public partial Task BanAsync(
        [SlashCommandParameter(Name = "user", Description = "The user to ban.")]
        GuildUser user,

        [SlashCommandParameter(Name = "reason", Description = "The reason for banning this user.")]
        string reason,

        [SlashCommandParameter(Name = "delete_messages", Description = "How much of their recent message history to delete.")]
        DeleteMessages deleteMessages,

        [SlashCommandParameter(Name = "silent", Description = "If true, does not broadcast this action to the server.")]
        bool silent = false,

        [SlashCommandParameter(Name = "dm", Description = "If true, sends the user a DM.")]
        bool dm = false
    );
    #endregion
    public async partial Task BanAsync(GuildUser user, string reason, DeleteMessages deleteMessages, bool silent, bool dm) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        if (dm) {
            await (await user.GetDMChannelAsync()).SendMessageAsync(new MessageProperties().WithEmbeds([
                EmbedHelper.Embed(
                    description: $"You have been banned from **{Context.Guild!.Name}** for **{reason}**.",
                    color: Colors.Orange
                )
            ]));
        }

        try {
            await user.BanAsync((int)deleteMessages, new RestRequestProperties().WithAuditLogReason($"Banned by {Context.User.Username} for: {reason}"));
        }
        catch (Exception e) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        description: $"Failed to ban <@{user.Id}>: {e.Message}",
                        color: Colors.Red
                    )
                ];
            });
            return;
        }

        AddInfraction(user.Id, GuildModeration.Infraction.Type.Ban, reason);

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    description: $"Done!",
                    color: Colors.Green
                )
            ];
        });

        await FollowupAsync(
            new InteractionMessageProperties()
            .WithEmbeds([
                EmbedHelper.Embed(
                    description: $"<@{user.Id}> has been banned for **{reason}**.",
                    color: Colors.Red
                )
            ])
        );
    }

    private void AddInfraction(ulong userId, GuildModeration.Infraction.Type type, string reason) {
        var infraction = new GuildModeration.Infraction {
            ModeratorId = Context.User.Id,
            Reason = reason,
            InfractionType = type,
            Timestamp = DateTime.UtcNow
        };

        // If the guild has a logs channel and moderation logs are enabled, send the infraction there.
        if (GuildSettings.TryGet(Context.Guild!.Id, out var guildSettings)) return;
        if (guildSettings.LogsChannelId != null && guildSettings.ModerationLogsEnabled) {
            var channel = Context.Guild.Channels[guildSettings.LogsChannelId!.Value];
            if (channel is TextGuildChannel textChannel) {
                textChannel.SendMessageAsync(new MessageProperties().WithEmbeds([
                    EmbedHelper.Embed(
                        title: $"Moderation Action: {type}",
                        color: type switch {
                            GuildModeration.Infraction.Type.Warning => Colors.Yellow,
                            GuildModeration.Infraction.Type.Timeout => Colors.Orange,
                            GuildModeration.Infraction.Type.Kick => Colors.Red,
                            GuildModeration.Infraction.Type.Ban => Colors.Red,
                            _ => Colors.White
                        },
                        fields: [
                            EmbedHelper.Field("User:", $"<@{userId}>", true),
                            EmbedHelper.Field("Moderator:", $"<@{Context.User.Id}>", true),
                            EmbedHelper.Field("Reason:", reason)
                        ],
                        timestamp: DateTime.UtcNow
                    )
                ]));
            }
        }

        GuildModeration.Upsert(Context.Guild!.Id, gm => {
            gm.ModeratedUsers.TryGetValue(userId, out var infractions);
            if (infractions == null) {
                infractions = [infraction];
                gm.ModeratedUsers[userId] = infractions;
            }
            else {
                infractions!.Add(infraction);
                gm.ModeratedUsers[userId] = infractions;
            }
        });
    }
}
