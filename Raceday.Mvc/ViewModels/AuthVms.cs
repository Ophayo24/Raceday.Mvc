using System.ComponentModel.DataAnnotations;

namespace Raceday.Mvc.ViewModels;

public class RegisterVm
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
    [Required] public string Role { get; set; } = "Participant";
    [MaxLength(20)] public string? Phone { get; set; }
}

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Message { get; set; } = "";
    public string Role { get; set; } = "";
    public int UserId { get; set; }
}

public class UserProfileVm
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? Phone { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class UpdateProfileVm
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [MaxLength(20)] public string? Phone { get; set; }
    [MaxLength(500)] public string? ProfilePictureUrl { get; set; }
}


