using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Project Name is mandatory!")]
        [StringLength(100, ErrorMessage = "Project Name cannot have more than 35 characters.")]
        [MinLength(5, ErrorMessage = "Project Name must have at least 5 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Project Description is mandatory!")]

        public string? Description { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
        public string? UserId { get; set; } // organiser's id 
        public virtual Team? Team { get; set; }
        public virtual ApplicationUser? User { get; set; } 

        public virtual ICollection<Task>? Tasks { get; set; }

    }
}
