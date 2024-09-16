using System.Security.Claims;
using API.Data;
using API.Dtos.Comment;
using API.Dtos.Like;
using API.Dtos.Post;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{

    private readonly BlobService _blobService;
    private readonly ApplicationDBContext _context;

    public PostController(BlobService blobService, ApplicationDBContext context)
        {
            _blobService = blobService;
            _context = context;
        }

    // GET: api/Post// GET: api/Post
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        var totalPosts = await _context.Posts.CountAsync();

        if (totalPosts == 0)
        {
            return NotFound("No posts found");
        }

        var posts = await _context
            .Posts.Include(p => p.AppUser)
            .Include(p => p.Comments)
            .ThenInclude(c => c.AppUser)
            .Include(p => p.Likes)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostResponseDto
            {
                Id = p.Id,
                Caption = p.Caption,
                ImageUrl = p.ImageUrl,
                AccountName = p.AppUser.AccountName,
                Likes = p
                    .Likes.Select(l => new LikeResponseDto
                    {
                        Id = l.Id,
                        AppUserId = l.AppUserId,
                        AccountName = l.AppUser.AccountName,
                        CreatedDate = l.CreatedDate,
                    })
                    .ToList(),
                Comments = p
                    .Comments.Select(c => new CommentResponseDto
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
                    .ToList(),
                CategoryName = p.Category.Name,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
            })
            .ToListAsync();

        var paginationMetadata = new
        {
            totalCount = totalPosts,
            pageSize,
            currentPage = pageNumber,
            totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
        };

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Metadata = new { CurrentUserId = userId, pagination = paginationMetadata };

        Response.Headers.Append("Metadata", Newtonsoft.Json.JsonConvert.SerializeObject(Metadata));

        return Ok(posts);
    }

    // GET: api/Post/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var post = await _context
            .Posts.Include(p => p.AppUser)
            .Include(p => p.Comments)
            .ThenInclude(c => c.AppUser) // Ensure to fetch comment user details
            .Include(p => p.Likes)
            .Include(p => p.Category)
            .Where(p => p.Id == id) // Filter by post ID
            .Select(p => new PostResponseDto
            {
                Id = p.Id,
                Caption = p.Caption,
                ImageUrl = p.ImageUrl,
                AccountName = p.AppUser.AccountName,
                Comments = p
                    .Comments.Select(c => new CommentResponseDto
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
                    .ToList(),
                CategoryName = p.Category.Name,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
            })
            .FirstOrDefaultAsync(); // Return a single post, not a list

        if (post == null)
        {
            return NotFound("No post found");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Metadata = new { CurrentUserId = userId };

        Response.Headers.Append("Metadata", Newtonsoft.Json.JsonConvert.SerializeObject(Metadata));

        return Ok(post);
    }

    // GET: api/Post/Category/5
    [HttpGet("Category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCategory(
        int categoryId,
        int pageNumber = 1,
        int pageSize = 10
    )
    {
        var totalPosts = await _context.Posts.Where(p => p.CategoryId == categoryId).CountAsync();

        var category = await _context.Categories.FindAsync(categoryId);

        if (category == null)
        {
            return BadRequest("Invalid category");
        }

        if (totalPosts == 0)
        {
            return NotFound("No posts found for this category");
        }

        var posts = await _context
            .Posts.Where(p => p.CategoryId == categoryId)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Likes)
            .Include(p => p.Likes)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostResponseDto
            {
                Id = p.Id,
                Caption = p.Caption,
                ImageUrl = p.ImageUrl,
                AccountName = p.AppUser.AccountName,
                Likes = p
                    .Likes.Select(l => new LikeResponseDto
                    {
                        Id = l.Id,
                        AppUserId = l.AppUserId,
                        AccountName = l.AppUser.AccountName,
                        CreatedDate = l.CreatedDate,
                    })
                    .ToList(),
                Comments = p
                    .Comments.Select(c => new CommentResponseDto
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
                    .ToList(),
                CategoryName = p.Category.Name,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
            })
            .ToListAsync();

        var paginationMetadata = new
        {
            totalCount = totalPosts,
            pageSize,
            currentPage = pageNumber,
            totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
        };

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Metadata = new { CurrentUserId = userId, pagination = paginationMetadata };

        Response.Headers.Append("Metadata", Newtonsoft.Json.JsonConvert.SerializeObject(Metadata));

        return Ok(posts);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromForm] PostDto postDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        var category = await _context.Categories.FindAsync(postDto.CategoryId);

        if (category == null && postDto.CategoryId.HasValue)
        {
            return BadRequest("Invalid category");
        }


        var ImageUrl = await _blobService.UploadImageAsync(postDto.Image);

        var post = new Post
        {
            Caption = postDto.Caption,
            ImageUrl = ImageUrl,
            CategoryId = postDto?.CategoryId,
            AppUserId = userId,
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Fetch the AppUser to ensure it's not null
        post = await _context
            .Posts.Include(p => p.AppUser)
            .FirstOrDefaultAsync(p => p.Id == post.Id);

        if (post.AppUser == null)
        {
            return BadRequest("Invalid user");
        }

        var newPost = new PostResponseDto
        {
            Id = post.Id,
            Caption = post.Caption,
            ImageUrl = post.ImageUrl,
            AccountName = post.AppUser.AccountName,
            CategoryName = category?.Name,
            CreatedDate = post.CreatedDate,
            UpdatedDate = post.UpdatedDate,
        };

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, newPost);
    }

    // PUT: api/Post/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, [FromForm] PostDto updatedPost)
    {
        if (updatedPost == null)
        {
            return BadRequest("Updated post data is null");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }

        var post = await _context
            .Posts.Include(p => p.AppUser)
            .Include(p => p.Likes)
            .ThenInclude(l => l.AppUser)
            .Include(p => p.Comments)
            .ThenInclude(c => c.AppUser)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Likes)
            .ThenInclude(l => l.AppUser)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound("Post not found");
        }

        if (post.AppUserId != userId)
        {
            return Unauthorized("User is not authorized to update this post");
        }

        Category? category = null;
        if (updatedPost.CategoryId.HasValue)
        {
            category = await _context.Categories.FindAsync(updatedPost.CategoryId.Value);
            if (category == null)
            {
                return BadRequest("Invalid category");
            }
        }

        string imageUrl = post.ImageUrl; // Keep the old image URL by default
        if (updatedPost.Image != null && updatedPost.Image.Length > 0)
        {
            imageUrl = await _blobService.UploadImageAsync(updatedPost.Image);
        }

        //TODO remove old image from blob storage

        post.Caption = updatedPost.Caption;
        post.ImageUrl = imageUrl;
        post.UpdatedDate = DateTime.UtcNow;
        post.CategoryId = updatedPost.CategoryId;


        var returnPost = new PostResponseDto
        {
            Id = post.Id,
            Caption = post.Caption,
            ImageUrl = post.ImageUrl,
            AccountName = post.AppUser.AccountName,
            CategoryName = category?.Name,
            CreatedDate = post.CreatedDate,
            UpdatedDate = post.UpdatedDate,
            Likes = post
                .Likes.Select(l => new LikeResponseDto
                {
                    Id = l.Id,
                    AppUserId = l.AppUserId,
                    AccountName = l.AppUser.AccountName,
                    CreatedDate = l.CreatedDate,
                })
                .ToList(),
            Comments = post
                .Comments.Select(c => new CommentResponseDto
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
                .ToList(),
        };

        _context.Posts.Update(post);
        await _context.SaveChangesAsync();

        return Ok(returnPost);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchPost(int id, [FromForm] PostUpdateDto updatedPost)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized("User is not authenticated");
        }

        var post = await _context.Posts
            .Include(p => p.Likes)
            .ThenInclude(l => l.AppUser)
            .Include(p => p.Comments)
            .ThenInclude(c => c.AppUser)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Likes)
            .ThenInclude(l => l.AppUser)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound("Post not found");
        }

        if (post.AppUserId != userId)
        {
            return Unauthorized("User is not authorized to update this post");
        }

        Category? category = null;
        if (updatedPost.CategoryId.HasValue)
        {
            category = await _context.Categories.FindAsync(updatedPost.CategoryId.Value);
            if (category == null)
            {
                return BadRequest("Invalid category");
            }
        }

        string imageUrl = post.ImageUrl; // Keep the old image URL by default
        if (updatedPost.Image != null && updatedPost.Image.Length > 0)
        {
            imageUrl = await _blobService.UploadImageAsync(updatedPost.Image);
            // TODO: Remove old image from blob storage
        }

        if (!string.IsNullOrEmpty(updatedPost.Caption))
        {
            post.Caption = updatedPost.Caption;
        }

        post.ImageUrl = imageUrl;
        post.UpdatedDate = DateTime.UtcNow;
        post.CategoryId = updatedPost.CategoryId;

        _context.Posts.Update(post);
        await _context.SaveChangesAsync();

        var returnPost = new PostResponseDto
{
    Id = post.Id,
    Caption = post.Caption,
    ImageUrl = post.ImageUrl,
    AccountName = post.AppUser?.AccountName ?? "Unknown",
    CategoryName = category?.Name ?? "Uncategorized",
    CreatedDate = post.CreatedDate,
    UpdatedDate = post.UpdatedDate,
    Likes = post.Likes?.Select(l => new LikeResponseDto
    {
        Id = l.Id,
        AppUserId = l.AppUserId,
        AccountName = l.AppUser?.AccountName ?? "Unknown",
        CreatedDate = l.CreatedDate,
    }).ToList() ?? new List<LikeResponseDto>(),
    Comments = post.Comments?.Select(c => new CommentResponseDto
    {
        Id = c.Id,
        CommentText = c.Text, // Ensure this matches the actual property name in the Comment class
        AccountName = c.AppUser?.AccountName ?? "Unknown",
        CreatedDate = c.CreatedDate,
        Likes = c.Likes?.Select(l => new LikeResponseDto
        {
            Id = l.Id,
            AppUserId = l.AppUserId,
            AccountName = l.AppUser?.AccountName ?? "Unknown",
            CreatedDate = l.CreatedDate,
        }).ToList() ?? new List<LikeResponseDto>()
    }).ToList() ?? new List<CommentResponseDto>()
};

return Ok(returnPost);
    }

    // DELETE: api/Post/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var post = await _context
            .Posts.Include(p => p.Likes)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound("Post not found");
        }

        var isAdmin = User.IsInRole("Admin");

        if (post.AppUserId != userId && !isAdmin)
        {
            return Unauthorized();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
