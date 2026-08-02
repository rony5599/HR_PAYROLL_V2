using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class OrganizationalUnit : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? ParentUnitId { get; set; }
    public OrganizationalUnit? ParentUnit { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public OrganizationalUnitLevel Level { get; set; } = OrganizationalUnitLevel.Department;
    public Guid? HeadEmployeeId { get; set; }
    public Employee? HeadEmployee { get; set; }
    public string? CostCenter { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<OrganizationalUnit> ChildUnits { get; set; } = new List<OrganizationalUnit>();
}

public class Designation : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Grade : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;
}

public class EmploymentType : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class EmployeeCategory : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
