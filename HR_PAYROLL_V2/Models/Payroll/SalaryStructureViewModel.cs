using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Payroll;

public class SalaryStructureViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string PayrollFrequency { get; set; } = "Monthly";

    public ProrationMethod ProrationMethod { get; set; } = ProrationMethod.WorkingDays;

    [DataType(DataType.Date)]
    public DateOnly EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public bool IsActive { get; set; } = true;

    public List<Guid> SelectedComponentIds { get; set; } = new();
}
