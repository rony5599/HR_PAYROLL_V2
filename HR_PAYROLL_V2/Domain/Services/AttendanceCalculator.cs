using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Services;

public record AttendanceCalculationResult(decimal WorkedHours, int LateMinutes, int EarlyExitMinutes, AttendanceStatus Status);

public static class AttendanceCalculator
{
    public static bool IsWeekend(DateOnly date, WorkingDays weekendDays)
    {
        var dayFlag = date.DayOfWeek switch
        {
            DayOfWeek.Sunday => WorkingDays.Sunday,
            DayOfWeek.Monday => WorkingDays.Monday,
            DayOfWeek.Tuesday => WorkingDays.Tuesday,
            DayOfWeek.Wednesday => WorkingDays.Wednesday,
            DayOfWeek.Thursday => WorkingDays.Thursday,
            DayOfWeek.Friday => WorkingDays.Friday,
            DayOfWeek.Saturday => WorkingDays.Saturday,
            _ => WorkingDays.None
        };

        return weekendDays.HasFlag(dayFlag);
    }


    public static AttendanceCalculationResult Calculate(Shift? shift, TimeOnly? checkIn, TimeOnly? checkOut, bool isProtectedDay = false)
    {
        if (checkIn is null || checkOut is null)
        {
            return new AttendanceCalculationResult(0, 0, 0, isProtectedDay ? AttendanceStatus.Holiday : AttendanceStatus.Incomplete);
        }

        if (isProtectedDay && shift is not null && !shift.HonorHolidayProtection)
        {
            isProtectedDay = false;
        }

        var totalMinutes = (checkOut.Value.ToTimeSpan() - checkIn.Value.ToTimeSpan()).TotalMinutes;
        if (shift is not null && shift.CrossesMidnight && totalMinutes < 0)
        {
            totalMinutes += 24 * 60;
        }

        var breakMinutes = shift?.BreakMinutes ?? 0;
        var workedHours = Math.Max(0m, (decimal)(totalMinutes - breakMinutes) / 60m);

        if (isProtectedDay)
        {
            return new AttendanceCalculationResult(Math.Round(workedHours, 2), 0, 0, AttendanceStatus.Holiday);
        }

        if (shift is null)
        {
            return new AttendanceCalculationResult(Math.Round(workedHours, 2), 0, 0, AttendanceStatus.Present);
        }

        var lateMinutes = 0;
        var startDiff = (checkIn.Value.ToTimeSpan() - shift.StartTime.ToTimeSpan()).TotalMinutes;
        if (startDiff > shift.LateGraceMinutes)
        {
            lateMinutes = (int)Math.Round(startDiff);
        }

        var earlyExitMinutes = 0;
        var endDiff = (shift.EndTime.ToTimeSpan() - checkOut.Value.ToTimeSpan()).TotalMinutes;
        if (endDiff > shift.EarlyExitGraceMinutes)
        {
            earlyExitMinutes = (int)Math.Round(endDiff);
        }

        var status = AttendanceStatus.Present;
        if (workedHours <= shift.HalfDayHours)
        {
            status = AttendanceStatus.HalfDay;
        }
        else if (lateMinutes > 0)
        {
            status = AttendanceStatus.Late;
        }
        else if (earlyExitMinutes > 0)
        {
            status = AttendanceStatus.EarlyDeparture;
        }

        return new AttendanceCalculationResult(Math.Round(workedHours, 2), lateMinutes, earlyExitMinutes, status);
    }
}
