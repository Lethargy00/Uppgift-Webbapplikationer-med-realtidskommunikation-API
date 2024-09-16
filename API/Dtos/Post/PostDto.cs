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
        public string Caption { get; set; }
        public string ImageUrl { get; set; }
        public int? CategoryId { get; set; }
    }
}
