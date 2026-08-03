using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Role;

public class RoleViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public List<Guid> SelectedPermissionIds { get; set; } = new();
}
