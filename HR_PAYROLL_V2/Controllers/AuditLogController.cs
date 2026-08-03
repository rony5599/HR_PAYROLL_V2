using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Authorization;
using HR_PAYROLL_V2.Models.AuditLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class AuditLogController : Controller
{
    private const int PageSize = 50;

    private readonly IUnitOfWork _unitOfWork;

    public AuditLogController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [RequirePermission("AuditLogs.View")]
    public async Task<IActionResult> Index(string? entityName, AuditAction? actionType, DateOnly? from, DateOnly? to, int page = 1)
    {
        var query = _unitOfWork.AuditLogs.Query();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName == entityName);
        }

        if (actionType.HasValue)
        {
            query = query.Where(a => a.Action == actionType);
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.Timestamp >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(a => a.Timestamp <= toUtc);
        }

        query = query.OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync();
        page = Math.Max(page, 1);
        var totalPages = Math.Max((int)Math.Ceiling(totalCount / (double)PageSize), 1);
        page = Math.Min(page, totalPages);

        var items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        var entityNames = await _unitOfWork.AuditLogs.Query()
            .Select(a => a.EntityName)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();

        return View(new AuditLogListViewModel
        {
            Items = items,
            EntityNames = entityNames,
            EntityName = entityName,
            Action = actionType,
            From = from,
            To = to,
            Page = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        });
    }

    [RequirePermission("AuditLogs.View")]
    public async Task<IActionResult> Details(Guid id)
    {
        var log = await _unitOfWork.AuditLogs.GetByIdAsync(id);
        if (log is null)
        {
            return NotFound();
        }

        return View(log);
    }
}
