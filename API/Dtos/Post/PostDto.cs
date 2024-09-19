using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using API.Models;

namespace API.Dtos.Post
{
    public class PostDto
    {
        [Required, MaxLength(280, ErrorMessage = "Posts can't exceed 280 characters")]
        public string Caption { get; set; }

        [Required]
        public IFormFile Image { get; set; }
        public int? CategoryId { get; set; }
    }
}
