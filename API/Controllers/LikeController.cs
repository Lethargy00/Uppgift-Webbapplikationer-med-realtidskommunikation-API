using System.Security.Claims;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LikeController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public LikeController(ApplicationDBContext context)
    {
        _context = context;
    }

    // POST: api/Like/Post/5
    [HttpPost("Post/{postId}")]
    public async Task<IActionResult> LikePost(int postId)
    {
        if (postId <= 0)
        {
            return BadRequest("Post id is required");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized("You must be logged in to like a post.");
        }

        try
        {
            var post = await _context
                .Posts.Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            if (post.Likes.Any(l => l.AppUserId == userId))
            {
                return Conflict(new { message = "You have already liked this post." });
            }

            var like = new Like { PostId = postId, AppUserId = userId };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post liked successfully." });
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while saving the like.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    // POST: api/Like/Comment/5
    [HttpPost("Comment/{commentId}")]
    public async Task<IActionResult> LikeComment(int commentId)
    {
        if (commentId <= 0)
        {
            return BadRequest("Comment id must be greater than zero.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized("You must be logged in to like a comment.");
        }

        try
        {
            var comment = await _context
                .Comments.Include(c => c.Likes)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
            {
                return NotFound("Comment not found");
            }

            if (comment.Likes.Any(l => l.AppUserId == userId))
            {
                return Conflict(new { message = "You have already liked this comment." });
            }

            var like = new Like { CommentId = commentId, AppUserId = userId };
            comment.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment liked successfully." });
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while saving the like.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    // DELETE: api/Like/5
    [HttpDelete("{postId}")]
    public async Task<IActionResult> Unlike(int postId)
    {
        if (postId <= 0)
        {
            return BadRequest("Post id must be greater than zero.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized("You must be logged in to unlike a post.");
        }

        try
        {
            var post = await _context
                .Posts.Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return NotFound("Post not found");
            }

            var like = await _context.Likes.FirstOrDefaultAsync(l =>
                l.PostId == postId && l.AppUserId == userId
            );

            if (like == null)
            {
                return Conflict("You have not liked this post.");
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while updating the like.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpDelete("Comment/{commentId}")]
    public async Task<IActionResult> UnlikeComment(int commentId)
    {
        if (commentId <= 0)
        {
            return BadRequest("Comment id must be greater than zero.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized("You must be logged in to unlike a comment.");
        }

        try
        {
            var like = await _context
                .Likes.Include(l => l.Comment)
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.AppUserId == userId);

            if (like == null)
            {
                var commentExists = await _context.Comments.AnyAsync(c => c.Id == commentId);
                if (!commentExists)
                {
                    return NotFound("Comment not found.");
                }
                return Conflict("You have not liked this comment.");
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while updating the like.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
