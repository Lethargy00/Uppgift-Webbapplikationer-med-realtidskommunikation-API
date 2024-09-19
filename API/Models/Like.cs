using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        [ForeignKey("Post")]
        public int? PostId { get; set; }
        public Post? Post { get; set; }

        [ForeignKey("Comment")]
        public int? CommentId { get; set; }
        public Comment? Comment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
