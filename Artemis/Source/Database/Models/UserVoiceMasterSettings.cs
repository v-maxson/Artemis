using NetCord;

namespace Database.Models;

internal class UserVoiceMasterSettings : DatabaseModel<UserVoiceMasterSettings>
{
    public required string ChannelName { get; set; }
    public int ChannelLimit { get; set; }

    public static UserVoiceMasterSettings Default(User user)
    {
        return new UserVoiceMasterSettings
        {
            Id = user.Id,
            ChannelName = $"{user.Username}'s Channel",
            ChannelLimit = 0
        };
    }
}
