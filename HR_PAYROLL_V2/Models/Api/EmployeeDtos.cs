using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Api;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public Guid? OrganizationalUnitId { get; set; }
    public string? OrganizationalUnitName { get; set; }
    public Guid? DesignationId { get; set; }
    public string? DesignationTitle { get; set; }
    public DateOnly DateOfJoining { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? WorkLocation { get; set; }
    public string? PhotoUrl { get; set; }
}

public class EmployeeCreateRequest
{
    [Required, MaxLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? Phone { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    public Guid? OrganizationalUnitId { get; set; }
    public Guid? DesignationId { get; set; }
    public Guid? GradeId { get; set; }
    public Guid? EmploymentTypeId { get; set; }
    public Guid? EmployeeCategoryId { get; set; }

    [Required]
    public DateOnly DateOfJoining { get; set; }

    public EmployeeStatus Status { get; set; } = EmployeeStatus.OnProbation;
    public string? WorkLocation { get; set; }
}

public class EmployeeUpdateRequest : EmployeeCreateRequest
{
}
