using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TaskManagementApp.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required(ErrorMessage="Please add your first name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Please add your last name")]
        public string? LastName { get; set; }
        public virtual ICollection<TeamMember>? TeamMembers { get; set; }
        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
