namespace DB.Models;

internal class GuildRoleMenus : DatabaseModel<GuildRoleMenus>
{
    public class RoleMenu
    {
        public List<ulong> RoleIds { get; set; } = [];
        public bool MultiSelect { get; set; }
    }

    public Dictionary<string, RoleMenu> RoleMenus { get; set; } = [];
}
