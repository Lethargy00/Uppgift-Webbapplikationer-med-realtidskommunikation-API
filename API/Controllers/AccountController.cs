using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("add-admin-role/{email}")]
        public async Task<IActionResult> AddAdminRole(string email)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

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

        [HttpDelete("remove-admin-role/{email}")]
        public async Task<IActionResult> RemoveAdminRole(string email)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(
                    new { message = "You are not authorized to perform this action." }
                );
            }
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

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

        [HttpGet("WhoAmI")]
        public async Task<IActionResult> WhoAmI()
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                var user = await _userManager.GetUserAsync(User);
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
    }
}
