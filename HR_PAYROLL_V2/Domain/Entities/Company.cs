using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public string PayrollCurrency { get; set; } = "USD";
    public string PayrollFrequency { get; set; } = "Monthly";
    public string TimeZone { get; set; } = "UTC";
    public WorkingDays WeekendDays { get; set; } = WorkingDays.Friday | WorkingDays.Saturday;
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<OrganizationalUnit> OrganizationalUnits { get; set; } = new List<OrganizationalUnit>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class Branch : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
