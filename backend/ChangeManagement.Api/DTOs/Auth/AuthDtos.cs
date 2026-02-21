namespace ChangeManagement.Api.DTOs.Auth;

public class LoginRequestDto
{
    public string Upn { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserAdminDto User { get; set; } = new();
}

public class UserAdminDto
{
    public Guid Id { get; set; }
    public string Upn { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string Upn { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}
