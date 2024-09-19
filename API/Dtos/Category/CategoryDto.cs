using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Category
{
    public class CategoryDto
    {
        [Required, MaxLength(12, ErrorMessage = "Category name can't exceed 12 characters")]
        public string Name { get; set; }
    }
}
