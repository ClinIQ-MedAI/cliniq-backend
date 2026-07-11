using Clinic.Authentication.Authorization;
using Clinic.Infrastructure.Entities.Enums;
using Clinic.Infrastructure.Extensions;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Booking.Doctor.Controllers;

[ApiController]
[Route("doctor/performance")]
[Authorize(Policy = PolicyNames.ActiveDoctor)]
public class BookingDoctorPerformanceController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPerformance(CancellationToken cancellationToken)
    {
        var doctorId = User.GetUserId()!;
        var today = DateTime.UtcNow;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        var doctorBookings = dbContext.Bookings
            .Where(b => b.DoctorSchedule.DoctorId == doctorId);

        // 1. Total unique patients (ever)
        var totalPatients = await doctorBookings
            .Select(b => b.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Total unique patients up to end of last month
        var totalPatientsLastMonth = await doctorBookings
            .Where(b => b.DoctorSchedule.Date <= DateOnly.FromDateTime(lastMonthEnd))
            .Select(b => b.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);

        double totalPatientsChangePercent = 0;
        if (totalPatientsLastMonth > 0)
        {
            totalPatientsChangePercent = Math.Round(
                ((totalPatients - totalPatientsLastMonth) / (double)totalPatientsLastMonth) * 100, 1);
        }

        // 2. New patients this month (first booking with this doctor was this month)
        var currentMonthStartDateOnly = DateOnly.FromDateTime(currentMonthStart);
        var lastMonthStartDateOnly = DateOnly.FromDateTime(lastMonthStart);

        var newPatientsThisMonth = await doctorBookings
            .Where(b => b.DoctorSchedule.Date >= currentMonthStartDateOnly)
            .GroupBy(b => b.PatientId)
            .Where(g => g.Min(b => b.DoctorSchedule.Date) >= currentMonthStartDateOnly)
            .CountAsync(cancellationToken);

        var newPatientsLastMonth = await doctorBookings
            .Where(b => b.DoctorSchedule.Date >= lastMonthStartDateOnly
                      && b.DoctorSchedule.Date < currentMonthStartDateOnly)
            .GroupBy(b => b.PatientId)
            .Where(g => g.Min(b => b.DoctorSchedule.Date) >= lastMonthStartDateOnly
                     && g.Min(b => b.DoctorSchedule.Date) < currentMonthStartDateOnly)
            .CountAsync(cancellationToken);

        double newPatientsChangePercent = 0;
        if (newPatientsLastMonth > 0)
        {
            newPatientsChangePercent = Math.Round(
                ((newPatientsThisMonth - newPatientsLastMonth) / (double)newPatientsLastMonth) * 100, 1);
        }

        // 3. Monthly performance (last 6 months)
        var monthlyPerformance = new List<MonthlyPerformanceDto>();

        for (int i = 5; i >= 0; i--)
        {
            var targetMonthStart = currentMonthStart.AddMonths(-i);
            var targetMonthEnd = targetMonthStart.AddMonths(1).AddTicks(-1);
            var startDateOnly = DateOnly.FromDateTime(targetMonthStart);
            var endDateOnly = DateOnly.FromDateTime(targetMonthEnd);

            var completedCount = await doctorBookings
                .CountAsync(b => b.Status == BookingStatus.COMPLETED
                              && b.DoctorSchedule.Date >= startDateOnly
                              && b.DoctorSchedule.Date <= endDateOnly,
                            cancellationToken);

            var canceledCount = await doctorBookings
                .CountAsync(b => b.Status == BookingStatus.CANCELLED
                              && b.DoctorSchedule.Date >= startDateOnly
                              && b.DoctorSchedule.Date <= endDateOnly,
                            cancellationToken);

            monthlyPerformance.Add(new MonthlyPerformanceDto(
                targetMonthStart.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                completedCount,
                canceledCount));
        }

        return Ok(new DoctorPerformanceResponse(
            totalPatients,
            totalPatientsChangePercent,
            newPatientsThisMonth,
            newPatientsChangePercent,
            monthlyPerformance));
    }
}

public record MonthlyPerformanceDto(string Month, int Completed, int Canceled);

public record DoctorPerformanceResponse(
    int TotalPatients,
    double TotalPatientsChangePercent,
    int NewPatientsThisMonth,
    double NewPatientsChangePercent,
    List<MonthlyPerformanceDto> MonthlyPerformance);
