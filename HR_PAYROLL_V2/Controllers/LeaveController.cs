using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class LeaveController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public LeaveController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(LeaveStatus? status)
    {
        var query = _unitOfWork.LeaveApplications.Query()
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Include(l => l.Approver)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        ViewBag.Status = status;
        return View(await query.OrderByDescending(l => l.CreatedAt).ToListAsync());
    }

    private static decimal CalculateDays(DateOnly start, DateOnly end, bool excludeWeekends)
    {
        if (end < start)
        {
            return 0;
        }

        decimal days = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (excludeWeekends && (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday))
            {
                continue;
            }
            days++;
        }

        return days;
    }

    private async Task<decimal> GetRemainingBalanceAsync(Guid employeeId, LeaveType leaveType)
    {
        var yearStart = new DateOnly(DateTime.UtcNow.Year, 1, 1);
        var used = (await _unitOfWork.LeaveApplications.FindAsync(l =>
                l.EmployeeId == employeeId &&
                l.LeaveTypeId == leaveType.Id &&
                l.Status == LeaveStatus.Approved &&
                l.StartDate >= yearStart))
            .Sum(l => l.Days);

        return leaveType.AnnualEntitlementDays - used;
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.Employees = new SelectList(await _unitOfWork.Employees.GetAllAsync(), "Id", "FullName");
        ViewBag.LeaveTypes = new SelectList(await _unitOfWork.LeaveTypes.FindAsync(l => l.IsActive), "Id", "Name");
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new LeaveApplicationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveApplicationViewModel model)
    {
        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(model.LeaveTypeId);
        if (leaveType is null)
        {
            ModelState.AddModelError(nameof(model.LeaveTypeId), "Leave type not found.");
        }

        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date cannot be before the start date.");
        }

        decimal days = 0;
        if (leaveType is not null && model.EndDate >= model.StartDate)
        {
            days = CalculateDays(model.StartDate, model.EndDate, leaveType.ExcludeWeekends);

            if (days > leaveType.MaxConsecutiveDays)
            {
                ModelState.AddModelError(string.Empty, $"{leaveType.Name} cannot exceed {leaveType.MaxConsecutiveDays} consecutive days.");
            }

            var noticeDays = (model.StartDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days;
            if (noticeDays < leaveType.AdvanceNoticeDays)
            {
                ModelState.AddModelError(nameof(model.StartDate), $"{leaveType.Name} requires at least {leaveType.AdvanceNoticeDays} days advance notice.");
            }

            var remaining = await GetRemainingBalanceAsync(model.EmployeeId, leaveType);
            if (days > remaining)
            {
                ModelState.AddModelError(string.Empty, $"Insufficient leave balance. Remaining balance: {remaining} day(s).");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var application = new LeaveApplication
        {
            EmployeeId = model.EmployeeId,
            LeaveTypeId = model.LeaveTypeId,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Days = days,
            Reason = model.Reason,
            Status = LeaveStatus.Pending
        };

        await _unitOfWork.LeaveApplications.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Leave application submitted for approval.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
    public async Task<IActionResult> Approve(LeaveDecisionViewModel model)
    {
        var application = await _unitOfWork.LeaveApplications.GetByIdAsync(model.Id);
        if (application is null)
        {
            return NotFound();
        }

        application.Status = LeaveStatus.Approved;
        application.DecidedAt = DateTime.UtcNow;
        application.DecisionComment = model.Comment;
        _unitOfWork.LeaveApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Leave application approved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
    public async Task<IActionResult> Reject(LeaveDecisionViewModel model)
    {
        var application = await _unitOfWork.LeaveApplications.GetByIdAsync(model.Id);
        if (application is null)
        {
            return NotFound();
        }

        application.Status = LeaveStatus.Rejected;
        application.DecidedAt = DateTime.UtcNow;
        application.DecisionComment = model.Comment;
        _unitOfWork.LeaveApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Leave application rejected.";
        return RedirectToAction(nameof(Index));
    }
}
