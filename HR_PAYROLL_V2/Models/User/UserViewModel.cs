using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.User;

public class UserViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }

    public Guid? CompanyId { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Guid> SelectedRoleIds { get; set; } = new();
}
