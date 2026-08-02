using HR_PAYROLL_V2.Domain.Common;

namespace HR_PAYROLL_V2.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }

    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Employee? Employee { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public Guid PermissionId { get; set; }
    public Permission? Permission { get; set; }
}

