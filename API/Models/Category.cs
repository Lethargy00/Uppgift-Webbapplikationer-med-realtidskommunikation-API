using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(25)]
        public string Name { get; set; }
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
