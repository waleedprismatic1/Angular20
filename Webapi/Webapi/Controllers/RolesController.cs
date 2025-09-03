using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Data;
using Webapi.Models;
using   Webapi.Services;


namespace Webapi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public RolesController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

        }

        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles
                
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,   // ✅ Extra column
                    r.CreatedAt,     // ✅ Already present
                    Permissions = _context.RolePermissions
                        .Where(rp => rp.RoleId == r.Id)
                        .Select(rp => new
                        {
                            rp.Permission.Name,
                            rp.Permission.Module,
                            rp.Permission.HierarchyLevel,
                            rp.CanRead,
                            rp.CanWrite,
                            rp.CanUpdate,
                            rp.CanDelete,
                            rp.CanPrint,
                            rp.CanView
                        }).ToList()
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpPost("PostRole")]
        public async Task<IActionResult> PostRole([FromBody] RoleCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Role name is required");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ AspNetRoles me role create
                var identityRole = new ApplicationRole
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow,
                    NormalizedName = model.Name.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                var result = await _roleManager.CreateAsync(identityRole);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                // 2️⃣ RolePermissions insert using AspNetRoles.Id
                if (model.Permissions != null && model.Permissions.Any())
                {
                    var rolePermissions = model.Permissions.Select(p => new RolePermission
                    {
                        RoleId = identityRole.Id, // dbo.Roles skip, direct AspNetRoles.Id
                        PermissionId = p.PermissionId,
                        CanRead = p.CanRead,
                        CanWrite = p.CanWrite,
                        CanUpdate = p.CanUpdate,
                        CanDelete = p.CanDelete,
                        CanPrint = p.CanPrint,
                        CanView = p.CanView
                    }).ToList();

                    _context.RolePermissions.AddRange(rolePermissions);
                    await _context.SaveChangesAsync();
                }

                // 3️⃣ Commit transaction
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetRole), new { id = identityRole.Id }, identityRole);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error creating role: {ex.Message}");
            }
        }






        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(string id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    Permissions = r.RolePermissions.Select(rp => new
                    {
                        rp.PermissionId,
                        rp.Permission.Name,
                        rp.Permission.Module,
                        rp.Permission.HierarchyLevel,
                        rp.CanRead,
                        rp.CanWrite,
                        rp.CanUpdate,
                        rp.CanDelete,
                        rp.CanPrint,
                        rp.CanView
                    }),
                    r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleUpdateModel model)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.Name = model.Name;
            role.Description = model.Description;

            _context.Roles.Update(role);

            // Update permissions
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingPermissions);

            if (model.Permissions != null)
            {
                foreach (var permission in model.Permissions)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.PermissionId,
                        CanRead = permission.CanRead,
                        CanWrite = permission.CanWrite,
                        CanUpdate = permission.CanUpdate,
                        CanDelete = permission.CanDelete,
                        CanPrint = permission.CanPrint,
                        CanView = permission.CanView
                    };

                    _context.RolePermissions.Add(rolePermission);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(role);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok("Role deleted successfully");
        }
    }
    public class RoleCreateModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RolePermissionModel> Permissions { get; set; }
    }

    public class RoleUpdateModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RolePermissionModel> Permissions { get; set; }
    }

    public class RolePermissionModel
    {
        public Guid PermissionId { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
        public bool CanView { get; set; }
    }
}
