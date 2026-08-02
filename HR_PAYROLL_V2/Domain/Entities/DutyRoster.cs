using HR_PAYROLL_V2.Domain.Common;

namespace HR_PAYROLL_V2.Domain.Entities;

public class DutyRoster : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public Guid ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public Guid? OrganizationalUnitId { get; set; }
    public OrganizationalUnit? OrganizationalUnit { get; set; }

    public bool IsLocked { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<DutyRosterMember> Members { get; set; } = new List<DutyRosterMember>();
}

public class DutyRosterMember : BaseEntity
{
    public Guid DutyRosterId { get; set; }
    public DutyRoster? DutyRoster { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
