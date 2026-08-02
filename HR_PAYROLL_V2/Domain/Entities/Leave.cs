using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class LeaveType : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal AnnualEntitlementDays { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int AdvanceNoticeDays { get; set; }
    public bool RequiresAttachment { get; set; }
    public bool ExcludeWeekends { get; set; } = true;
    public bool ExcludeHolidays { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class LeaveApplication : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid LeaveTypeId { get; set; }
    public LeaveType? LeaveType { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal Days { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public Guid? ApproverId { get; set; }
    public Employee? Approver { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionComment { get; set; }
}
