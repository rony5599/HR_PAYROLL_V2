namespace HR_PAYROLL_V2.Models.Api;

public class PayslipDto
{
    public Guid PayrollRecordId { get; set; }
    public Guid PayrollPeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly PaymentDate { get; set; }

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
