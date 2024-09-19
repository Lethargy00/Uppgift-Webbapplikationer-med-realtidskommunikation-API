using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Text { get; set; }

        [Required]
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post Post { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public List<Like> Likes { get; set; } = new List<Like>();

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
