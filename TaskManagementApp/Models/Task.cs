using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? status { get; set; }
        public string? UserId { get; set; }
        public int? ProjectId { get; set; }   

        public virtual Project? Project { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
