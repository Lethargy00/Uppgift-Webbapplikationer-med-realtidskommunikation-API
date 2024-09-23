using System.Security.Claims;
using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userList = users
                    .Select(user => new
                    {
                        user.Id,
                        user.AccountName,
                        IsAdmin = _userManager.IsInRoleAsync(user, "Admin").Result,
                        user.Email,
                    })
                    .ToList();

                return Ok(userList);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("WhoAmI")]
        public async Task<IActionResult> WhoAmI()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var isAdmin = User.IsInRole("Admin");

                return Ok(
                    new
                    {
                        user.Id,
                        user.AccountName,
                        isAdmin,
                    }
                );
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    AccountName = model.AccountName,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(new { Message = "Registration successfully." });
                }
                else
                {
                    if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                    {
                        return Conflict(new { Error = "Email already in use." });
                    }
                    return BadRequest(ModelState);
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "An error occurred during registration." });
            }
        }

        [HttpPost("admin/{id}")]
        public async Task<IActionResult> AddAdminRole(string id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var roleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExists)
                {
                    return StatusCode(500, new { Error = "Role not found." });
                }

                var userInRole = await _userManager.IsInRoleAsync(user, "Admin");
                if (userInRole)
                {
                    return Conflict(new { Error = "User already has Admin role." });
                }

                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Admin role added successfully." });
                }
                else
                {
                    return StatusCode(500, new { Error = "Failed to add Admin role." });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> RemoveAdminRole(string id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (id == currentUserId)
                {
                    return StatusCode(
                        403,
                        new { Error = "You cannot perform this action on your own account." }
                    );
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { Error = "User not found." });
                }

                var roleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExists)
                {
                    return StatusCode(500, new { Error = "Role not found." });
                }

                var userInRole = await _userManager.IsInRoleAsync(user, "Admin");
                if (!userInRole)
                {
                    return BadRequest(new { Error = "User does not have Admin role." });
                }

                var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Admin role removed successfully." });
                }
                else
                {
                    return StatusCode(500, new { Error = "Failed to remove Admin role." });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isUserAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isUserAdmin)
            {
                return Forbid();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }
    }
}
