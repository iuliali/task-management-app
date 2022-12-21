using MessagePack;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace TaskManagementApp.Models
{
    public class TeamMember
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }
        public int? TeamId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual Team? Team { get; set; }
    }
}
