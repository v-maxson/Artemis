namespace Database.Models;

internal class GuildSettings : DatabaseModel<GuildSettings>
{
    // Voice Master
    public ulong? VoiceMasterChannelId { get; set; }

    // Auto Role
    public ulong? AutoRoleId { get; set; }

    // Logging
    public ulong? LogsChannelId { get; set; }
    public bool ModerationLogsEnabled { get; set; }
    public bool JoinLeaveLogsEnabled { get; set; }
    public bool MessageEditLogsEnabled { get; set; }
    public bool MessageDeleteLogsEnabled { get; set; }
}