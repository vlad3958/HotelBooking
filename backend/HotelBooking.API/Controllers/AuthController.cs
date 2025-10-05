using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

 

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        // Всегда создаём обычного пользователя с ролью User.
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        // Гарантируем что роль только User при публичной регистрации.
        if (!await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        return Ok(new { Message = "User registered", user.Id, user.Email });
    }

    [HttpPost("admin/create-user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminCreateUser(AdminCreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and Password are required");

        var normalizedRole = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim();
        var allowedRoles = new[] { "User", "Admin" };
        if (!allowedRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Invalid role");

        var user = new ApplicationUser { UserName = request.Email, Email = request.Email, IsAdmin = normalizedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) };
        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return BadRequest(createResult.Errors.Select(e => e.Description));

        // Ensure role exists
        if (!await _userManager.IsInRoleAsync(user, normalizedRole))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (!addRoleResult.Succeeded)
                return BadRequest(addRoleResult.Errors.Select(e => e.Description));
        }

        return Ok(new { Message = "User created by admin", user.Id, user.Email, Role = normalizedRole });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { Error = "Користувача з таким email не знайдено (not registered)" });
        }

        var valid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!valid.Succeeded)
        {
            return Unauthorized(new { Error = "Невірний пароль" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);
        return Ok(new { token });
    }

    private string GenerateJwt(ApplicationUser user, IList<string> roles)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key")!;
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");
        var expiresMinutes = jwtSection.GetValue<int>("ExpiresMinutes");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
   public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Роль убрана из публичной регистрации, всегда User
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AdminCreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Admin может явно указать роль
    }