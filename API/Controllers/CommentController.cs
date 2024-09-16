using System.Security.Claims;
using API.Data;
using API.Dtos.Comment;
using API.Dtos.Like;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public CommentController(ApplicationDBContext context)
    {
        _context = context;
    }

    // POST: api/Comment
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment(int postId, [FromBody] CommentDto commentDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        var post = await _context.Posts.FindAsync(postId);

        if (post == null)
        {
            return NotFound("Post not found");
        }

        var newComment = new Comment
        {
            Text = commentDto.Text,
            PostId = postId,
            AppUserId = userId,
        };

        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();

        // Reload the comment with the AppUser navigation property
        newComment = await _context
            .Comments.Include(c => c.AppUser)
            .FirstOrDefaultAsync(c => c.Id == newComment.Id);

        if (newComment == null || newComment.AppUser == null)
        {
            return BadRequest("Failed to create comment");
        }

        var responseComment = new CommentResponseDto
        {
            Id = newComment.Id,
            CommentText = newComment.Text,
            AccountName = newComment.AppUser.AccountName,
            CreatedDate = newComment.CreatedDate,
            UpdatedDate = newComment.UpdatedDate,
            Likes = new List<LikeResponseDto>(),
        };

        return CreatedAtAction("GetComment", new { id = newComment.Id }, responseComment);
    }

    // GET: api/Comment/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComment(int id)
    {
        var comment = await _context
            .Comments.Include(c => c.Likes)
            .Where(c => c.Id == id)
            .Select(c => new CommentResponseDto
            {
                Id = c.Id,
                CommentText = c.Text,
                AccountName = c.AppUser.AccountName,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                Likes = c
                    .Likes.Select(l => new LikeResponseDto
                    {
                        Id = l.Id,
                        AppUserId = l.AppUserId,
                        AccountName = l.AppUser.AccountName,
                        CreatedDate = l.CreatedDate,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync();

        if (comment == null)
        {
            return NotFound("Comment not found");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Metadata = new { CurrentUserId = userId };
        Response.Headers.Append("Metadata", Newtonsoft.Json.JsonConvert.SerializeObject(Metadata));

        return Ok(comment);
    }

    // PUT: api/Comment/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] CommentDto updatedComment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comment = await _context
            .Comments.Include(c => c.AppUser)
            .Include(c => c.Likes)
            .ThenInclude(l => l.AppUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound("Comment not found");
        }

        if (comment.AppUserId != userId)
        {
            return Unauthorized();
        }

        comment.Text = updatedComment.Text;
        comment.UpdatedDate = DateTime.UtcNow;

        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();

        var returnComment = new CommentResponseDto
        {
            Id = comment.Id,
            CommentText = comment.Text,
            AccountName = comment.AppUser?.AccountName, // Use null-conditional operator
            CreatedDate = comment.CreatedDate,
            UpdatedDate = comment.UpdatedDate,
            Likes = comment
                .Likes.Select(l => new LikeResponseDto
                {
                    Id = l.Id,
                    AppUserId = l.AppUserId,
                    AccountName = l.AppUser?.AccountName, // Use null-conditional operator
                    CreatedDate = l.CreatedDate,
                })
                .ToList(),
        };

        return Ok(returnComment);
    }

    // DELETE: api/Comment/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comment = await _context.Comments.FindAsync(id);

        if (comment == null)
        {
            return NotFound("Comment not found");
        }

        var isAdmin = User.IsInRole("Admin");

        if (comment.AppUserId != userId && !isAdmin)
        {
            return Unauthorized();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
