using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Like
{
    public class LikeResponseDto
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public string AccountName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
