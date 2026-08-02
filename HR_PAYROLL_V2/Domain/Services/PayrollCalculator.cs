using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Services;

public record PayrollCalculationResult(
    decimal BasicAmount, decimal EarningsAmount, decimal ComponentDeductionAmount,
    decimal AttendanceDeductionAmount, decimal WorkHourDeductionAmount, decimal OvertimeAmount,
    decimal GrossPay, decimal NetPay,
    int PresentDays, int AbsentDays, int PaidLeaveDays, int UnpaidLeaveDays, int HolidayDays);

public class PayrollCalculationContext
{
    public const decimal StandardMonthlyHours = 208m;

    public required EmployeeSalaryAssignment Assignment { get; init; }
    public required SalaryStructure Structure { get; init; }
    public required IReadOnlyList<SalaryComponent> Components { get; init; }
    public required DateOnly PeriodStart { get; init; }
    public required DateOnly PeriodEnd { get; init; }
    public required IReadOnlyDictionary<DateOnly, Attendance> AttendanceByDate { get; init; }
    public required IReadOnlyList<(LeaveApplication Application, bool IsPaid)> ApprovedLeaves { get; init; }
    public required WorkingDays WeekendDays { get; init; }
    public required IReadOnlyCollection<DateOnly> HolidayDates { get; init; }
    public required IReadOnlyDictionary<Guid, Shift> ShiftsById { get; init; }
    public IReadOnlyList<OvertimeRequest> ApprovedOvertime { get; init; } = Array.Empty<OvertimeRequest>();
    public OvertimePolicy? OvertimePolicy { get; init; }
}

public static class PayrollCalculator
{
    public static PayrollCalculationResult Calculate(PayrollCalculationContext ctx)
    {
        var basic = ctx.Assignment.BasicSalary;

        decimal earnings = 0;
        decimal componentDeductions = 0;
        foreach (var component in ctx.Components)
        {
            var amount = component.Method == SalaryCalculationMethod.PercentageOfBasic
                ? basic * component.Value / 100m
                : component.Value;

            if (component.Type == SalaryComponentType.Earning)
            {
                earnings += amount;
            }
            else
            {
                componentDeductions += amount;
            }
        }

        var daysInMonth = DateTime.DaysInMonth(ctx.PeriodStart.Year, ctx.PeriodStart.Month);
        var dailyRate = ctx.Structure.ProrationMethod switch
        {
            ProrationMethod.CalendarDays => basic / daysInMonth,
            ProrationMethod.WorkingDays => basic / Math.Max(1, CountWorkingDaysInMonth(ctx.PeriodStart, ctx.WeekendDays, ctx.HolidayDates)),
            _ => basic / 30m
        };
        var hourlyRate = basic / PayrollCalculationContext.StandardMonthlyHours;

        decimal attendanceDeduction = 0;
        decimal workHourDeduction = 0;
        int presentDays = 0, absentDays = 0, paidLeaveDays = 0, unpaidLeaveDays = 0, holidayDays = 0;

        for (var date = ctx.PeriodStart; date <= ctx.PeriodEnd; date = date.AddDays(1))
        {
            if (ctx.AttendanceByDate.TryGetValue(date, out var attendance))
            {
                switch (attendance.Status)
                {
                    case AttendanceStatus.Holiday:
                        holidayDays++;
                        break;
                    case AttendanceStatus.HalfDay:
                        presentDays++;
                        attendanceDeduction += dailyRate / 2m;
                        break;
                    case AttendanceStatus.Incomplete:
                        absentDays++;
                        attendanceDeduction += dailyRate;
                        break;
                    default:
                        presentDays++;
                        if (attendance.ShiftId is Guid shiftId && ctx.ShiftsById.TryGetValue(shiftId, out var shift))
                        {
                            var shortfall = shift.RequiredHours - attendance.WorkedHours;
                            if (shortfall > 0)
                            {
                                workHourDeduction += shortfall * hourlyRate;
                            }
                        }
                        break;
                }
                continue;
            }

            if (AttendanceCalculator.IsWeekend(date, ctx.WeekendDays) || ctx.HolidayDates.Contains(date))
            {
                holidayDays++;
                continue;
            }

            var leaveForDate = ctx.ApprovedLeaves.FirstOrDefault(l => date >= l.Application.StartDate && date <= l.Application.EndDate);
            if (leaveForDate.Application is not null)
            {
                if (leaveForDate.IsPaid)
                {
                    paidLeaveDays++;
                }
                else
                {
                    unpaidLeaveDays++;
                    attendanceDeduction += dailyRate;
                }
                continue;
            }

            absentDays++;
            attendanceDeduction += dailyRate;
        }

        decimal overtimeAmount = 0;
        var normalMultiplier = ctx.OvertimePolicy?.NormalMultiplier ?? 1.5m;
        var weekendMultiplier = ctx.OvertimePolicy?.WeekendMultiplier ?? 2.0m;
        var holidayMultiplier = ctx.OvertimePolicy?.HolidayMultiplier ?? 2.0m;

        foreach (var overtime in ctx.ApprovedOvertime)
        {
            var isHolidayOrWeekend = AttendanceCalculator.IsWeekend(overtime.OvertimeDate, ctx.WeekendDays) || ctx.HolidayDates.Contains(overtime.OvertimeDate);
            var multiplier = ctx.AttendanceByDate.TryGetValue(overtime.OvertimeDate, out var otAttendance) && otAttendance.Status == AttendanceStatus.Holiday
                ? holidayMultiplier
                : isHolidayOrWeekend ? weekendMultiplier : normalMultiplier;

            overtimeAmount += overtime.Hours * hourlyRate * multiplier;
        }

        var gross = basic + earnings + overtimeAmount;
        var net = gross - componentDeductions - attendanceDeduction - workHourDeduction;

        return new PayrollCalculationResult(
            Math.Round(basic, 2), Math.Round(earnings, 2), Math.Round(componentDeductions, 2),
            Math.Round(attendanceDeduction, 2), Math.Round(workHourDeduction, 2), Math.Round(overtimeAmount, 2),
            Math.Round(gross, 2), Math.Round(net, 2),
            presentDays, absentDays, paidLeaveDays, unpaidLeaveDays, holidayDays);
    }

    private static int CountWorkingDaysInMonth(DateOnly anyDateInMonth, WorkingDays weekendDays, IReadOnlyCollection<DateOnly> holidayDates)
    {
        var days = DateTime.DaysInMonth(anyDateInMonth.Year, anyDateInMonth.Month);
        var count = 0;
        for (var d = 1; d <= days; d++)
        {
            var date = new DateOnly(anyDateInMonth.Year, anyDateInMonth.Month, d);
            if (!AttendanceCalculator.IsWeekend(date, weekendDays) && !holidayDates.Contains(date))
            {
                count++;
            }
        }

        return count;
    }
}
