using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class SalaryComponent : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public SalaryComponentType Type { get; set; } = SalaryComponentType.Earning;
    public SalaryCalculationMethod Method { get; set; } = SalaryCalculationMethod.FixedAmount;
    public decimal Value { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class SalaryStructure : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string PayrollFrequency { get; set; } = "Monthly";
    public ProrationMethod ProrationMethod { get; set; } = ProrationMethod.WorkingDays;
    public DateOnly EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SalaryStructureComponent> Components { get; set; } = new List<SalaryStructureComponent>();
}

public class SalaryStructureComponent : BaseEntity
{
    public Guid SalaryStructureId { get; set; }
    public SalaryStructure? SalaryStructure { get; set; }
    public Guid SalaryComponentId { get; set; }
    public SalaryComponent? SalaryComponent { get; set; }
}

public class EmployeeSalaryAssignment : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid SalaryStructureId { get; set; }
    public SalaryStructure? SalaryStructure { get; set; }

    public decimal BasicSalary { get; set; }
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
