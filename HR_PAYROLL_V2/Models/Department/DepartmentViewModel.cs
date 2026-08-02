using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Department;

public class DepartmentViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    public OrganizationalUnitLevel Level { get; set; } = OrganizationalUnitLevel.Department;
    public Guid? ParentUnitId { get; set; }
    public Guid? HeadEmployeeId { get; set; }
    public string? CostCenter { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
