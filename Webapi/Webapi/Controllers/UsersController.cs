using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Webapi.Data;
using Webapi.Models;

namespace Webapi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet("GetUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // <-- correct Identity way
                result.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.UserName,
                    Roles = roles,
                    user.CreatedAt
                });
            }

            return Ok(result);
        }





        [HttpPost("CreateUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if username already exists
            if (await _userManager.FindByNameAsync(request.UserName) != null)
                return BadRequest("Username already exists");

            // Check if email already exists
            if (await _userManager.FindByEmailAsync(request.Email) != null)
                return BadRequest("Email already exists");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign roles to the user
            if (request.RoleIds != null && request.RoleIds.Any())
            {
                foreach (var roleId in request.RoleIds)
                {
                    var role = await _context.Roles.FindAsync(roleId);
                    if (role == null)
                    {
                        return BadRequest($"Role with Id '{roleId}' does not exist."); // explicit error
                    }

                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            // Return the created user data
            var createdUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == user.Id)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUser);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Users can only get their own info unless they're admin
            if (id != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateModel model)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Users can only update their own info unless they're admin
            if (id != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if email is being changed to an existing email
            if (user.Email != model.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmail != null && existingEmail.Id != id)
                {
                    return BadRequest("Email already exists");
                }
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Return updated user data
            var updatedUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == user.Id)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.UserName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            return Ok(updatedUser);
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

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User deleted successfully");
        }
    }

    public class UserUpdateModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }
    }

    public class UserCreateRequest
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public List<string> RoleIds { get; set; }
    }
}