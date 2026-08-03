using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Api;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public OrganizationalUnitLevel Level { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ParentUnitId { get; set; }
    public bool IsActive { get; set; }
}

public class DepartmentCreateRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public OrganizationalUnitLevel Level { get; set; } = OrganizationalUnitLevel.Department;

    [Required]
    public Guid CompanyId { get; set; }

    public Guid? ParentUnitId { get; set; }
}

public class DepartmentUpdateRequest : DepartmentCreateRequest
{
    public bool IsActive { get; set; } = true;
}
