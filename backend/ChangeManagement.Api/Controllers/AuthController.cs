using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChangeManagement.Api.Data;
using ChangeManagement.Api.DTOs.Auth;
using ChangeManagement.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ChangeManagementDbContext dbContext, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var upn = request.Upn.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Upn.ToLower() == upn, cancellationToken);

        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login for {Upn}", request.Upn);
            return Unauthorized("Invalid credentials.");
        }

        var token = CreateToken(user.UserId, user.Upn, user.Role);
        return Ok(new LoginResponseDto
        {
            Token = token,
            User = new UserAdminDto { Id = user.UserId, Upn = user.Upn, DisplayName = user.DisplayName, Role = user.Role, IsActive = user.IsActive }
        });
    }

    private string CreateToken(Guid userId, string upn, string role)
    {
        var key = _configuration["Jwt:Key"] ?? "local-dev-super-secret-key-change-me";
        var issuer = _configuration["Jwt:Issuer"] ?? "ChangeManagement.Api";
        var audience = _configuration["Jwt:Audience"] ?? "ChangeManagement.Frontend";
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Upn, upn),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
