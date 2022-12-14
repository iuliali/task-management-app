using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Team name is mandatory!")]
        [StringLength(100, ErrorMessage = "Team name cannot have more than 35 characters.")]
        [MinLength(5, ErrorMessage = "Team name must have at least 5 characters.")]
        public string? Name { get; set; }
        public virtual int? ProjectId { get; set; }

        public virtual Project? Project{ get; set; }
        public virtual ICollection<TeamMember>? TeamMembers { get; set; }
    }
}
