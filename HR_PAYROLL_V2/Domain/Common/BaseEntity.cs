namespace HR_PAYROLL_V2.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class EffectiveDatedEntity : BaseEntity
{
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
