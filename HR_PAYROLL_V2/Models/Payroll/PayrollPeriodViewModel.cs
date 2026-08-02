using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Payroll;

public class PayrollPeriodViewModel
{
    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

    [Required, DataType(DataType.Date)]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, DataType(DataType.Date)]
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}
