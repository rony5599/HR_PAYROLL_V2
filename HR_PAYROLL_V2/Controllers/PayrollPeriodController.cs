using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class PayrollPeriodController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public PayrollPeriodController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var periods = await _unitOfWork.PayrollPeriods.Query()
            .Include(p => p.Company)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        return View(periods);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Companies = new SelectList(await _unitOfWork.Companies.GetAllAsync(), "Id", "Name");
        return View(new PayrollPeriodViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayrollPeriodViewModel model)
    {
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after the start date.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Companies = new SelectList(await _unitOfWork.Companies.GetAllAsync(), "Id", "Name", model.CompanyId);
            return View(model);
        }

        await _unitOfWork.PayrollPeriods.AddAsync(new PayrollPeriod
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            PaymentDate = model.PaymentDate
        });
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Payroll period created.";
        return RedirectToAction(nameof(Index));
    }
}
