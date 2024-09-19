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
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { Error = "An error occurred while processing your request." }
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
                    return NotFound(new { message = "Category not found." });
                }

                return Ok(new { category.Id, category.Name });
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { message = "An error occurred while processing your request." }
                );
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> AddCategory(CategoryDto categoryDto)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingCategory = await _context.Categories.FirstOrDefaultAsync(c =>
                    c.Name.ToLower() == categoryDto.Name.ToLower()
                );

                if (existingCategory != null)
                {
                    return Conflict(
                        new { message = "A category with the same name already exists." }
                    );
                }

                var category = new Category { Name = categoryDto.Name };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = category.Id },
                    new { category.Id, category.Name }
                );
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "An error occurred while saving the category.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred while creating the category.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, CategoryDto categoryDto)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound(new { message = "Category not found." });
                }

                var existingCategory = await _context.Categories.FirstOrDefaultAsync(c =>
                    c.Name.ToLower() == categoryDto.Name.ToLower()
                );

                if (existingCategory != null)
                {
                    return Conflict(
                        new { message = "A category with the same name already exists." }
                    );
                }

                category.Name = categoryDto.Name;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return Ok(new { category.Id, category.Name });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "An error occurred while saving the category.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred while updating the category.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }

            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound(new { message = "Category not found." });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Category deleted successfully." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "An error occurred while deleting the category.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred while deleting the category.");
            }
        }
    }
}
