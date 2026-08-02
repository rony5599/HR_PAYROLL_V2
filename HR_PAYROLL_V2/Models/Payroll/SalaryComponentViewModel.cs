using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Payroll;

public class SalaryComponentViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    public SalaryComponentType Type { get; set; } = SalaryComponentType.Earning;
    public SalaryCalculationMethod Method { get; set; } = SalaryCalculationMethod.FixedAmount;

    [Range(0, 1000000)]
    public decimal Value { get; set; }

    public bool IsTaxable { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
