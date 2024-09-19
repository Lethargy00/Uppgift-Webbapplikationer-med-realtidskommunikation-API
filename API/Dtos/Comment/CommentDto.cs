using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Comment
{
    public class CommentDto
    {
        [Required, MaxLength(200, ErrorMessage = "Comments can't exceed 200 characters")]
        public string Text { get; set; }
    }
}
