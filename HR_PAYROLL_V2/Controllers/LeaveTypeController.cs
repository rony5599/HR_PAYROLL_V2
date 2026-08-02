using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Caching;
using HR_PAYROLL_V2.Models.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class LeaveTypeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyLookupService _companyLookup;

    public LeaveTypeController(IUnitOfWork unitOfWork, ICompanyLookupService companyLookup)
    {
        _unitOfWork = unitOfWork;
        _companyLookup = companyLookup;
    }

    public async Task<IActionResult> Index()
    {
        var leaveTypes = await _unitOfWork.LeaveTypes.Query().OrderBy(l => l.Name).ToListAsync();
        return View(leaveTypes);
    }

    private async Task PopulateDropdownsAsync(Guid? companyId = null)
    {
        ViewBag.Companies = new SelectList(await _companyLookup.GetAllAsync(), "Id", "Name", companyId);
    }

    public async Task<IActionResult> Create()
    {
        var model = new LeaveTypeViewModel();
        var firstCompany = (await _companyLookup.GetAllAsync()).FirstOrDefault();
        if (firstCompany is not null)
        {
            model.CompanyId = firstCompany.Id;
        }

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveTypeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        await _unitOfWork.LeaveTypes.AddAsync(new LeaveType
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Code = model.Code,
            AnnualEntitlementDays = model.AnnualEntitlementDays,
            MaxConsecutiveDays = model.MaxConsecutiveDays,
            AdvanceNoticeDays = model.AdvanceNoticeDays,
            RequiresAttachment = model.RequiresAttachment,
            ExcludeWeekends = model.ExcludeWeekends,
            ExcludeHolidays = model.ExcludeHolidays,
            IsActive = model.IsActive
        });
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Leave type saved.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
        if (leaveType is null)
        {
            return NotFound();
        }

        var model = new LeaveTypeViewModel
        {
            Id = leaveType.Id,
            CompanyId = leaveType.CompanyId,
            Name = leaveType.Name,
            Code = leaveType.Code,
            AnnualEntitlementDays = leaveType.AnnualEntitlementDays,
            MaxConsecutiveDays = leaveType.MaxConsecutiveDays,
            AdvanceNoticeDays = leaveType.AdvanceNoticeDays,
            RequiresAttachment = leaveType.RequiresAttachment,
            ExcludeWeekends = leaveType.ExcludeWeekends,
            ExcludeHolidays = leaveType.ExcludeHolidays,
            IsActive = leaveType.IsActive
        };

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LeaveTypeViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
        if (leaveType is null)
        {
            return NotFound();
        }

        leaveType.CompanyId = model.CompanyId;
        leaveType.Name = model.Name;
        leaveType.Code = model.Code;
        leaveType.AnnualEntitlementDays = model.AnnualEntitlementDays;
        leaveType.MaxConsecutiveDays = model.MaxConsecutiveDays;
        leaveType.AdvanceNoticeDays = model.AdvanceNoticeDays;
        leaveType.RequiresAttachment = model.RequiresAttachment;
        leaveType.ExcludeWeekends = model.ExcludeWeekends;
        leaveType.ExcludeHolidays = model.ExcludeHolidays;
        leaveType.IsActive = model.IsActive;

        _unitOfWork.LeaveTypes.Update(leaveType);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Leave type updated.";
        return RedirectToAction(nameof(Index));
    }
}
