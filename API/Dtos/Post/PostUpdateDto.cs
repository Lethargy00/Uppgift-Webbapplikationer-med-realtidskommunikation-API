using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Post
{
    public class PostUpdateDto
    {
        [MaxLength(280, ErrorMessage = "Posts can't exceed 280 characters")]
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
        public int? CategoryId { get; set; }
    }
}
