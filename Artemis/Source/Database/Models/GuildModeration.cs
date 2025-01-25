namespace Artemis.DB.Models;

internal class GuildModeration : DatabaseModel<GuildModeration>
{
    public Dictionary<ulong, List<Infraction>> ModeratedUsers { get; set; } = [];

    public class Infraction
    {
        public enum Type
        {
            Warning,
            Timeout,
            Kick,
            Ban
        }

        public ulong ModeratorId { get; set; }
        public required string Reason { get; set; }
        public Type InfractionType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
