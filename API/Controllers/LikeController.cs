using System.Security.Claims;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class LikeController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public LikeController(ApplicationDBContext context)
    {
        _context = context;
    }

    // POST: api/Like/Post/5
    [HttpPost("Post/{postId}")]
    [Authorize]
    public async Task<IActionResult> LikePost(int postId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var post = await _context
            .Posts.Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null)
        {
            return NotFound("Post not found");
        }

        if (post.Likes.Any(l => l.AppUserId == userId))
        {
            return BadRequest("You have already liked this post");
        }
        if (postId == 0)
        {
            return BadRequest("Post id is required");
        }

        var like = new Like { PostId = postId, AppUserId = userId };
        _context.Likes.Add(like);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // POST: api/Like/Comment/5
    [HttpPost("Comment/{commentId}")]
    [Authorize]
    public async Task<IActionResult> LikeComment(int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var comment = await _context
            .Comments.Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment == null)
        {
            return NotFound("Comment not found");
        }

        if (comment.Likes.Any(l => l.AppUserId == userId))
        {
            return BadRequest("You have already liked this comment");
        }

        if (commentId == 0)
        {
            return BadRequest("Comment id is required");
        }

        var like = new Like { CommentId = commentId, AppUserId = userId };
        comment.Likes.Add(like);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // DELETE: api/Like/5
    [HttpDelete("{postId}")]
    [Authorize]
    public async Task<IActionResult> Unlike(int postId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var post = await _context
            .Posts.Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
        {
            return NotFound("Post not found");
        }

        var like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId);

        if (like == null)
        {
            return NotFound("Post is not liked");
        }

        if (like.AppUserId != userId)
        {
            return Unauthorized();
        }

        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("Comment/{commentId}")]
    [Authorize]
    public async Task<IActionResult> UnlikeComment(int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var comment = await _context
            .Comments.Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return NotFound("Comment not found");
        }

        var like = await _context.Likes.FirstOrDefaultAsync(l => l.CommentId == commentId);

        if (like == null)
        {
            return NotFound("Comment is not liked");
        }

        if (like.AppUserId != userId)
        {
            return Unauthorized();
        }

        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
