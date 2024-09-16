using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace API.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(280)]
        public string Caption { get; set; }

        public string? ImageUrl { get; set; } // Nullable if no image

        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        // Relationships
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Like> Likes { get; set; } = new List<Like>();

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
