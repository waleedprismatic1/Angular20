using Microsoft.AspNetCore.Identity;
using Webapi.Models;

namespace Webapi.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtService _jwtService;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<string> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return _jwtService.GenerateJwtToken(user, roles);
        }

        public async Task<IdentityResult> Register(RegisterModel model)
        {
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded && model.RoleIds != null)
            {
                // Add user to selected roles
                foreach (var roleId in model.RoleIds)
                {
                    // You'll need to get role name from roleId
                    // This is a simplified version
                    await _userManager.AddToRoleAsync(user, roleId);
                }
            }

            return result;
        }
    }
}
