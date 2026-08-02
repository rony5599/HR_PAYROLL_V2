using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Shift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class DutyRosterController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DutyRosterController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var rosters = await _unitOfWork.DutyRosters.Query()
            .Include(r => r.Company)
            .Include(r => r.Shift)
            .Include(r => r.OrganizationalUnit)
            .Include(r => r.Members)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();

        return View(rosters);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var roster = await _unitOfWork.DutyRosters.Query()
            .Include(r => r.Shift)
            .Include(r => r.Company)
            .Include(r => r.Members).ThenInclude(m => m.Employee)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (roster is null)
        {
            return NotFound();
        }

        return View(roster);
    }

    private async Task PopulateDropdownsAsync(Guid? companyId = null)
    {
        ViewBag.Companies = new SelectList(await _unitOfWork.Companies.GetAllAsync(), "Id", "Name", companyId);
        ViewBag.Shifts = new SelectList(await _unitOfWork.Shifts.FindAsync(s => s.IsActive), "Id", "Name");
        ViewBag.Departments = new SelectList(await _unitOfWork.OrganizationalUnits.GetAllAsync(), "Id", "Name");
        ViewBag.Employees = await _unitOfWork.Employees.Query().Include(e => e.OrganizationalUnit).ToListAsync();
    }

    public async Task<IActionResult> Create()
    {
        var model = new DutyRosterViewModel();
        var firstCompany = (await _unitOfWork.Companies.GetAllAsync()).FirstOrDefault();
        if (firstCompany is not null)
        {
            model.CompanyId = firstCompany.Id;
        }

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DutyRosterViewModel model)
    {
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after the start date.");
        }

        if (!model.SelectedEmployeeIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Select at least one employee for this roster.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        var roster = new DutyRoster
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Code = model.Code,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            ShiftId = model.ShiftId,
            OrganizationalUnitId = model.OrganizationalUnitId,
            IsLocked = false
        };

        await _unitOfWork.DutyRosters.AddAsync(roster);
        await _unitOfWork.SaveChangesAsync();

        foreach (var employeeId in model.SelectedEmployeeIds.Distinct())
        {
            await _unitOfWork.DutyRosterMembers.AddAsync(new DutyRosterMember
            {
                DutyRosterId = roster.Id,
                EmployeeId = employeeId
            });
        }
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Duty roster created as a draft. Publish it to assign shifts.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(Guid id)
    {
        var roster = await _unitOfWork.DutyRosters.Query()
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (roster is null)
        {
            return NotFound();
        }

        if (roster.IsLocked)
        {
            TempData["Error"] = "This roster is already published.";
            return RedirectToAction(nameof(Details), new { id });
        }

        foreach (var member in roster.Members)
        {
            var activeShift = (await _unitOfWork.ShiftAssignments.FindAsync(
                a => a.EmployeeId == member.EmployeeId && a.IsActive)).FirstOrDefault();

            if (activeShift is not null)
            {
                if (activeShift.ShiftId == roster.ShiftId)
                {
                    continue;
                }

                activeShift.IsActive = false;
                activeShift.EffectiveEndDate = roster.StartDate.AddDays(-1);
                _unitOfWork.ShiftAssignments.Update(activeShift);
            }

            await _unitOfWork.ShiftAssignments.AddAsync(new ShiftAssignment
            {
                EmployeeId = member.EmployeeId,
                ShiftId = roster.ShiftId,
                EffectiveStartDate = roster.StartDate,
                EffectiveEndDate = roster.EndDate,
                IsActive = true
            });
        }

        roster.IsLocked = true;
        roster.PublishedAt = DateTime.UtcNow;
        _unitOfWork.DutyRosters.Update(roster);

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = $"Roster published. Shift assigned to {roster.Members.Count} employee(s).";
        return RedirectToAction(nameof(Details), new { id });
    }
}
