namespace GoogleGroups.Models
{
    public class GroupModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int? AccessLevel { get; set; }
        public List<ContactModel>? Members { get; set; }
        public List<GroupUserModel>? GroupUsers { get; set; }
        public List<string>? SuperAdmins { get; set; }
        public int? MembersCount { get; set; }
        public bool? IsMember { get; set; }
    }

    public class GroupUserModel
    {
        public string? Username { get; set; }
        public string? GroupId { get; set; }
        public int PermissionLevel { get; set; }
        public string? RoleTitle { get; set; }
    }
}
