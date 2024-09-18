using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos.Comment;
using API.Dtos.Like;

namespace API.Dtos.Post
{
    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public string ImageUrl { get; set; }
        public string AccountName { get; set; }
        public string AppUserId { get; set; }
        public List<LikeResponseDto> Likes { get; set; } = new List<LikeResponseDto>();
        public List<CommentResponseDto> Comments { get; set; } = new List<CommentResponseDto>();
        public string CategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
