using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Post
{
    public class PostUpdateDto
    {
        public string? Caption { get; set; }
        public IFormFile? Image { get; set; }
        public int? CategoryId { get; set; }
    }
}