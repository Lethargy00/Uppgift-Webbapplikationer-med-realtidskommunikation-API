using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Category
{
    public class CategoryDto
    {
        [Required]
        public string Name { get; set; }
    }
}
