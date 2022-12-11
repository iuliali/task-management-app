namespace TaskManagementApp.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }
        public int? TeamId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual Team? Team { get; set; }
    }
}
