using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Employee;

public class EmployeeViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
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
    public Guid? ReportingManagerId { get; set; }
    public Guid? ShiftId { get; set; }
    public Guid? SalaryStructureId { get; set; }

    [Range(0, 10000000)]
    public decimal? BasicSalary { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly DateOfJoining { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public EmployeeStatus Status { get; set; } = EmployeeStatus.OnProbation;
    public string? WorkLocation { get; set; }
}
