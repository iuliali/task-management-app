using Microsoft.AspNetCore.Identity;

namespace TaskManagementApp.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public virtual ICollection<TeamMember>? TeamMembers { get; set; }
    }
}
