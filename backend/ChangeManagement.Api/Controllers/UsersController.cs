using ChangeManagement.Api.Data;
using ChangeManagement.Api.Domain.Entities;
using ChangeManagement.Api.DTOs.Auth;
using ChangeManagement.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public class UsersController : ControllerBase
{
    private readonly ChangeManagementDbContext _dbContext;

    public UsersController(ChangeManagementDbContext dbContext) => _dbContext = dbContext;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserAdminDto>>> List(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users.OrderBy(x => x.Upn).ToListAsync(cancellationToken);
        return Ok(users.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<UserAdminDto>> Create([FromBody] CreateUserDto request, CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(x => x.Upn == request.Upn, cancellationToken))
        {
            return BadRequest("A user with this UPN already exists.");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Upn = request.Upn.Trim(),
            DisplayName = request.DisplayName.Trim(),
            Role = request.Role.Trim(),
            IsActive = true,
            PasswordHash = PasswordHasher.Hash(request.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserAdminDto>> Update(Guid id, [FromBody] UpdateUserDto request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == id, cancellationToken);
        if (user is null) return NotFound();

        user.Role = request.Role.Trim();
        user.IsActive = request.IsActive;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(user));
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == id, cancellationToken);
        if (user is null) return NotFound();

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Password reset." });
    }

    private static UserAdminDto ToDto(User user) => new()
    {
        Id = user.UserId,
        Upn = user.Upn,
        DisplayName = user.DisplayName,
        Role = user.Role,
        IsActive = user.IsActive
    };
}
