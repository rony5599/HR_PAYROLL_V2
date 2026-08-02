using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? PhotoUrl { get; set; }

    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public Guid? OrganizationalUnitId { get; set; }
    public OrganizationalUnit? OrganizationalUnit { get; set; }
    public Guid? DesignationId { get; set; }
    public Designation? Designation { get; set; }
    public Guid? GradeId { get; set; }
    public Grade? Grade { get; set; }
    public Guid? EmploymentTypeId { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public Guid? EmployeeCategoryId { get; set; }
    public EmployeeCategory? EmployeeCategory { get; set; }

    public DateOnly DateOfJoining { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ContractStartDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public int? ProbationPeriodMonths { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.OnProbation;
    public string? CostCenter { get; set; }
    public string? WorkLocation { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public ICollection<EmployeeReportingManager> ReportingRelationships { get; set; } = new List<EmployeeReportingManager>();
    public ICollection<EmployeeReportingManager> DirectReports { get; set; } = new List<EmployeeReportingManager>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class EmployeeReportingManager : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public ReportingRelationshipType RelationshipType { get; set; } = ReportingRelationshipType.Primary;
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
