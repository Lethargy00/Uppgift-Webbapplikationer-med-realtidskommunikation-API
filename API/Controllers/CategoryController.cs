using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Dtos.Category;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CategoryController(ApplicationDBContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAllCategories()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();

                if (categories == null || !categories.Any())
                {
                    return NotFound(new { message = "No categories found." });
                }

                return Ok(categories.Select(c => new { c.Id, c.Name }));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "An unexpected error occurred.", details = ex.Message }
                );
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound(new { message = $"Category with ID {id} not found." });
                }

                return Ok(new { category.Id, category.Name });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "An unexpected error occurred.", details = ex.Message }
                );
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> AddCategory(CategoryDto categoryDto)
        {
            if (string.IsNullOrEmpty(categoryDto.Name))
            {
                return BadRequest(new { message = "Category data is required." });
            }

            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }

            var category = new Category { Name = categoryDto.Name };

            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return Ok(new { category.Id, category.Name });
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(
                    500,
                    new
                    {
                        message = "An error occurred while updating the database.",
                        details = dbEx.Message,
                    }
                );
            }
            catch (Exception ex)
            {
                // Handle all other exceptions
                return StatusCode(
                    500,
                    new { message = "An unexpected error occurred.", details = ex.Message }
                );
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, CategoryDto categoryDto)
        {
            if (string.IsNullOrEmpty(categoryDto.Name))
            {
                return BadRequest(new { message = "Category data is required." });
            }

            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            category.Name = categoryDto.Name;

            try
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return Ok(new { category.Id, category.Name });
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(
                    500,
                    new
                    {
                        message = "An error occurred while updating the database.",
                        details = dbEx.Message,
                    }
                );
            }
            catch (Exception ex)
            {
                // Handle all other exceptions
                return StatusCode(
                    500,
                    new { message = "An unexpected error occurred.", details = ex.Message }
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Category deleted successfully." });
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(
                    500,
                    new
                    {
                        message = "An error occurred while updating the database.",
                        details = dbEx.Message,
                    }
                );
            }
            catch (Exception ex)
            {
                // Handle all other exceptions
                return StatusCode(
                    500,
                    new { message = "An unexpected error occurred.", details = ex.Message }
                );
            }
        }
    }
}
