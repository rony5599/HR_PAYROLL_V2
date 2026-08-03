using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;

namespace HR_PAYROLL_V2.Infrastructure.Caching;

public interface ICompanyLookupService
{
    Task<IReadOnlyList<Company>> GetAllAsync();
    Task InvalidateAsync();
}

public class CompanyLookupService : ICompanyLookupService
{
    private const string CacheKey = "lookup:companies";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CompanyLookupService(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    //public Task<IReadOnlyList<Company>> GetAllAsync() =>
    //    _cache.GetOrCreateAsync(CacheKey, async () => await _unitOfWork.Companies.GetAllAsync(), Ttl);

    public Task<IReadOnlyList<Company>> GetAllAsync() =>
    _cache.GetOrCreateAsync(
        CacheKey,
        () => _unitOfWork.Companies.GetAllAsync(),
        Ttl
    );

    public Task InvalidateAsync() => _cache.RemoveAsync(CacheKey);
}
