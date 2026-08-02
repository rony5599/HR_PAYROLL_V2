using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class PayrollPeriod : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly PaymentDate { get; set; }
    public PayrollPeriodStatus Status { get; set; } = PayrollPeriodStatus.Draft;

    public ICollection<PayrollRecord> Records { get; set; } = new List<PayrollRecord>();
}

public class PayrollRecord : BaseEntity
{
    public Guid PayrollPeriodId { get; set; }
    public PayrollPeriod? PayrollPeriod { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public decimal BasicAmount { get; set; }
    public decimal EarningsAmount { get; set; }
    public decimal ComponentDeductionAmount { get; set; }
    public decimal AttendanceDeductionAmount { get; set; }
    public decimal WorkHourDeductionAmount { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }

    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public int HolidayDays { get; set; }
}
