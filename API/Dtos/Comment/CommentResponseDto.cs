using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos.Like;
using API.Models;

namespace API.Dtos.Comment
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public string AccountName { get; set; }
        public string AppUserId { get; set; }
        public List<LikeResponseDto> Likes { get; set; } = new List<LikeResponseDto>();
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
