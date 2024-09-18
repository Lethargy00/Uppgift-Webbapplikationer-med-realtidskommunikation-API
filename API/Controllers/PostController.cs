using System.Security.Claims;
using API.Data;
using API.Dtos.Comment;
using API.Dtos.Like;
using API.Dtos.Post;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            var totalPosts = await _context.Posts.CountAsync();

            if (totalPosts == 0)
            {
                return NotFound("No posts found.");
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

            Response.Headers.Append(
                "Metadata",
                Newtonsoft.Json.JsonConvert.SerializeObject(Metadata)
            );

            return Ok(posts);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred while retrieving posts.");
        }
    }

    // GET: api/Post/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid post ID.");
        }

        try
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

            Response.Headers.Append(
                "Metadata",
                Newtonsoft.Json.JsonConvert.SerializeObject(Metadata)
            );

            return Ok(post);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred while retrieving the post.");
        }
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
        if (categoryId <= 0)
        {
            return BadRequest("Invalid category ID.");
        }

        try
        {
            var totalPosts = await _context
                .Posts.Where(p => p.CategoryId == categoryId)
                .CountAsync();

            var category = await _context.Categories.FindAsync(categoryId);

            if (category == null)
            {
                return BadRequest("Invalid category.");
            }

            if (totalPosts == 0)
            {
                return NotFound("No posts found for this category.");
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

            Response.Headers.Append(
                "Metadata",
                Newtonsoft.Json.JsonConvert.SerializeObject(Metadata)
            );

            return Ok(posts);
        }
        catch (Exception)
        {
            return StatusCode(
                500,
                "An unexpected error occurred while retrieving posts by category."
            );
        }
    }

    // POST: api/Post
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromForm] PostDto postDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        var category = await _context.Categories.FindAsync(postDto.CategoryId);

        var ImageUrl = await _blobService.UploadImageAsync(postDto.Image);

        var appUser = await _context.AppUsers.FindAsync(userId);

        if (appUser == null)
        {
            return StatusCode(500, "An unexpected error occurred while fetching user information");
        }

        var post = new Post
        {
            Caption = postDto.Caption,
            ImageUrl = ImageUrl,
            CategoryId = postDto.CategoryId,
            AppUserId = userId,
            AppUser = appUser,
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var newPost = new PostResponseDto
        {
            Id = post.Id,
            Caption = post.Caption,
            ImageUrl = post.ImageUrl,
            AccountName = post.AppUser.AccountName,
            CategoryName = post.Category.Name,
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        try
        {
            var post = await _context
                .Posts.Include(p => p.AppUser)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            if (post.AppUserId != userId)
            {
                return Unauthorized("User is not authorized to update this post.");
            }

            var category = await _context.Categories.FindAsync(updatedPost.CategoryId);

            if (category == null || updatedPost.CategoryId < 0)
            {
                return BadRequest("Invalid category");
            }

            string imageUrl = post.ImageUrl; // Keep the old image URL by default

            if (updatedPost.Image != null && updatedPost.Image.Length > 0)
            {
                var oldImageUrl = post.ImageUrl;

                imageUrl = await _blobService.UploadImageAsync(updatedPost.Image);

                if (imageUrl == string.Empty)
                {
                    return StatusCode(500, "An error occurred while updating the post image.");
                }

                if (oldImageUrl != null)
                {
                    await _blobService.DeleteBlobAsync(oldImageUrl);
                }
            }

            post.Caption = updatedPost.Caption;
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
                AccountName = post.AppUser.AccountName,
                CategoryName = category.Id == 0 ? "None" : category.Name,
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

            return Ok(returnPost);
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while updating the post.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred while updating the post.");
        }
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> PatchPost(int id, [FromForm] PostUpdateDto updatedPost)
    {
        if (updatedPost == null)
        {
            return BadRequest("Updated post data is null.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        try
        {
            var post = await _context
                .Posts.Include(p => p.AppUser)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            if (post.AppUserId != userId)
            {
                return Unauthorized("User is not authorized to update this post.");
            }

            // Update only the caption if provided
            if (!string.IsNullOrEmpty(updatedPost.Caption))
            {
                post.Caption = updatedPost.Caption;
            }

            // Update the image if provided
            if (updatedPost.Image != null && updatedPost.Image.Length > 0)
            {
                var oldImageUrl = post.ImageUrl;
                string imageUrl = await _blobService.UploadImageAsync(updatedPost.Image);
                post.ImageUrl = imageUrl;

                if (post.ImageUrl == null)
                {
                    return StatusCode(500, "An error occurred while updating the post image.");
                }

                if (oldImageUrl != null)
                {
                    await _blobService.DeleteBlobAsync(oldImageUrl);
                }
            }

            post.UpdatedDate = DateTime.UtcNow;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            var returnPost = new PostResponseDto
            {
                Id = post.Id,
                Caption = post.Caption,
                ImageUrl = post.ImageUrl,
                AccountName = post.AppUser.AccountName,
                CategoryName = post.Category.Id == 0 ? "None" : post.Category.Name,
                CreatedDate = post.CreatedDate,
                UpdatedDate = post.UpdatedDate,
                Likes =
                    post.Likes?.Select(l => new LikeResponseDto
                        {
                            Id = l.Id,
                            AppUserId = l.AppUserId,
                            AccountName = l.AppUser.AccountName,
                            CreatedDate = l.CreatedDate,
                        })
                        .ToList() ?? new List<LikeResponseDto>(),
                Comments =
                    post.Comments?.Select(c => new CommentResponseDto
                        {
                            Id = c.Id,
                            CommentText = c.Text,
                            AccountName = c.AppUser.AccountName,
                            CreatedDate = c.CreatedDate,
                            UpdatedDate = c.UpdatedDate,
                            Likes =
                                c.Likes?.Select(l => new LikeResponseDto
                                    {
                                        Id = l.Id,
                                        AppUserId = l.AppUserId,
                                        AccountName = l.AppUser.AccountName,
                                        CreatedDate = l.CreatedDate,
                                    })
                                    .ToList() ?? new List<LikeResponseDto>(),
                        })
                        .ToList() ?? new List<CommentResponseDto>(),
            };

            return Ok(returnPost);
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while updating the post.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred while updating the post.");
        }
    }

    // DELETE: api/Post/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid post ID.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
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
                return Unauthorized("User is not authorized to delete this post.");
            }

            var imageUrl = post.ImageUrl;
            if (imageUrl != null)
            {
                await _blobService.DeleteBlobAsync(imageUrl);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while deleting the post.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred while deleting the post.");
        }
    }
}
