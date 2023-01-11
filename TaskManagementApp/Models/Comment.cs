using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public int? TaskId { get; set; }
        [Required(ErrorMessage = "Comment content is mandatory !")]
        [StringLength(600, ErrorMessage = "Comment cannot have more then 600 characters.")]
        [MinLength(5, ErrorMessage = "Comment should have minimum 5 characters.")]
        public string? Content { get; set; }
        public string? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public virtual Task? Task { get; set; }

        public virtual ApplicationUser? User { get; set; }

    }
}
