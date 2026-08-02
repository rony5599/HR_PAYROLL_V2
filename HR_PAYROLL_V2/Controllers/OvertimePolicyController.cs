using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Caching;
using HR_PAYROLL_V2.Models.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class OvertimePolicyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyLookupService _companyLookup;

    public OvertimePolicyController(IUnitOfWork unitOfWork, ICompanyLookupService companyLookup)
    {
        _unitOfWork = unitOfWork;
        _companyLookup = companyLookup;
    }

    public async Task<IActionResult> Index()
    {
        var policies = await _unitOfWork.OvertimePolicies.Query().Include(p => p.Company).OrderBy(p => p.Name).ToListAsync();
        return View(policies);
    }

    private async Task PopulateDropdownsAsync(Guid? companyId = null)
    {
        ViewBag.Companies = new SelectList(await _companyLookup.GetAllAsync(), "Id", "Name", companyId);
    }

    public async Task<IActionResult> Create()
    {
        var model = new OvertimePolicyViewModel();
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
    public async Task<IActionResult> Create(OvertimePolicyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        await _unitOfWork.OvertimePolicies.AddAsync(new OvertimePolicy
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            MinOvertimeMinutes = model.MinOvertimeMinutes,
            MaxDailyHours = model.MaxDailyHours,
            MaxMonthlyHours = model.MaxMonthlyHours,
            NormalMultiplier = model.NormalMultiplier,
            WeekendMultiplier = model.WeekendMultiplier,
            HolidayMultiplier = model.HolidayMultiplier,
            RequiresApproval = model.RequiresApproval,
            IsActive = model.IsActive
        });
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Overtime policy saved.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var policy = await _unitOfWork.OvertimePolicies.GetByIdAsync(id);
        if (policy is null)
        {
            return NotFound();
        }

        var model = new OvertimePolicyViewModel
        {
            Id = policy.Id,
            CompanyId = policy.CompanyId,
            Name = policy.Name,
            MinOvertimeMinutes = policy.MinOvertimeMinutes,
            MaxDailyHours = policy.MaxDailyHours,
            MaxMonthlyHours = policy.MaxMonthlyHours,
            NormalMultiplier = policy.NormalMultiplier,
            WeekendMultiplier = policy.WeekendMultiplier,
            HolidayMultiplier = policy.HolidayMultiplier,
            RequiresApproval = policy.RequiresApproval,
            IsActive = policy.IsActive
        };

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OvertimePolicyViewModel model)
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

        var policy = await _unitOfWork.OvertimePolicies.GetByIdAsync(id);
        if (policy is null)
        {
            return NotFound();
        }

        policy.CompanyId = model.CompanyId;
        policy.Name = model.Name;
        policy.MinOvertimeMinutes = model.MinOvertimeMinutes;
        policy.MaxDailyHours = model.MaxDailyHours;
        policy.MaxMonthlyHours = model.MaxMonthlyHours;
        policy.NormalMultiplier = model.NormalMultiplier;
        policy.WeekendMultiplier = model.WeekendMultiplier;
        policy.HolidayMultiplier = model.HolidayMultiplier;
        policy.RequiresApproval = model.RequiresApproval;
        policy.IsActive = model.IsActive;

        _unitOfWork.OvertimePolicies.Update(policy);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Overtime policy updated.";
        return RedirectToAction(nameof(Index));
    }
}
