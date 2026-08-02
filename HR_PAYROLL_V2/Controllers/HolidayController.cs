using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class HolidayController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public HolidayController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var holidays = await _unitOfWork.Holidays.Query()
            .Include(h => h.Company)
            .OrderBy(h => h.Date)
            .ToListAsync();

        return View(holidays);
    }

    private async Task PopulateDropdownsAsync(Guid? companyId = null)
    {
        ViewBag.Companies = new SelectList(await _unitOfWork.Companies.GetAllAsync(), "Id", "Name", companyId);
    }

    public async Task<IActionResult> Create()
    {
        var model = new HolidayViewModel();
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
    public async Task<IActionResult> Create(HolidayViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        await _unitOfWork.Holidays.AddAsync(new Holiday
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Date = model.Date,
            Type = model.Type,
            Description = model.Description,
            IsActive = model.IsActive
        });
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Holiday added to the calendar.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var holiday = await _unitOfWork.Holidays.GetByIdAsync(id);
        if (holiday is null)
        {
            return NotFound();
        }

        var model = new HolidayViewModel
        {
            Id = holiday.Id,
            CompanyId = holiday.CompanyId,
            Name = holiday.Name,
            Date = holiday.Date,
            Type = holiday.Type,
            Description = holiday.Description,
            IsActive = holiday.IsActive
        };

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, HolidayViewModel model)
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

        var holiday = await _unitOfWork.Holidays.GetByIdAsync(id);
        if (holiday is null)
        {
            return NotFound();
        }

        holiday.CompanyId = model.CompanyId;
        holiday.Name = model.Name;
        holiday.Date = model.Date;
        holiday.Type = model.Type;
        holiday.Description = model.Description;
        holiday.IsActive = model.IsActive;

        _unitOfWork.Holidays.Update(holiday);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Holiday updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var holiday = await _unitOfWork.Holidays.GetByIdAsync(id);
        if (holiday is not null)
        {
            holiday.IsDeleted = true;
            _unitOfWork.Holidays.Update(holiday);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Holiday removed.";
        }

        return RedirectToAction(nameof(Index));
    }
}
