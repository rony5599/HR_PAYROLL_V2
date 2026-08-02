using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator")]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var companies = await _unitOfWork.Companies.GetAllAsync();
        return View(companies);
    }

    public IActionResult Create() => View(new CompanyViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _unitOfWork.Companies.ExistsAsync(c => c.Code == model.Code))
        {
            ModelState.AddModelError(nameof(model.Code), "A company with this code already exists.");
            return View(model);
        }

        var company = new Domain.Entities.Company
        {
            Name = model.Name,
            Code = model.Code,
            RegistrationNumber = model.RegistrationNumber,
            Address = model.Address,
            ContactEmail = model.ContactEmail,
            ContactPhone = model.ContactPhone,
            PayrollCurrency = model.PayrollCurrency,
            PayrollFrequency = model.PayrollFrequency,
            IsActive = model.IsActive,
            WeekendDays = model.ToWeekendDays()
        };

        await _unitOfWork.Companies.AddAsync(company);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Company created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is null)
        {
            return NotFound();
        }

        var model = new CompanyViewModel
        {
            Id = company.Id,
            Name = company.Name,
            Code = company.Code,
            RegistrationNumber = company.RegistrationNumber,
            Address = company.Address,
            ContactEmail = company.ContactEmail,
            ContactPhone = company.ContactPhone,
            PayrollCurrency = company.PayrollCurrency,
            PayrollFrequency = company.PayrollFrequency,
            IsActive = company.IsActive
        };
        model.FromWeekendDays(company.WeekendDays);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CompanyViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is null)
        {
            return NotFound();
        }

        if (await _unitOfWork.Companies.ExistsAsync(c => c.Code == model.Code && c.Id != id))
        {
            ModelState.AddModelError(nameof(model.Code), "A company with this code already exists.");
            return View(model);
        }

        company.Name = model.Name;
        company.Code = model.Code;
        company.RegistrationNumber = model.RegistrationNumber;
        company.Address = model.Address;
        company.ContactEmail = model.ContactEmail;
        company.ContactPhone = model.ContactPhone;
        company.PayrollCurrency = model.PayrollCurrency;
        company.PayrollFrequency = model.PayrollFrequency;
        company.IsActive = model.IsActive;
        company.WeekendDays = model.ToWeekendDays();

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Company updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is not null)
        {
            company.IsDeleted = true;
            _unitOfWork.Companies.Update(company);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Company removed.";
        }

        return RedirectToAction(nameof(Index));
    }
}
