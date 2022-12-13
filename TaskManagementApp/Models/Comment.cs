using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Comment content is mandatory !")]
        [StringLength(200, ErrorMessage = "Comment cannpt have more then 200 characters.")]
        [MinLength(5, ErrorMessage = "Comment should have minimum 5 characters.")]
        public string? Content { get; set; }
        public string? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ApplicationUser? User { get; set; }

    }
}
