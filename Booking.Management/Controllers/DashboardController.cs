using Clinic.Authentication.Authorization;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Booking.Management.Controllers;

[ApiController]
[Route("admin/dashboard")]
[Authorize(Policy = PolicyNames.Admin)]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public DashboardController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        // 1. Total Patients
        var totalPatients = await _dbContext.PatientProfiles.CountAsync(cancellationToken);

        // Patients count up to end of last month
        var totalPatientsLastMonth = await _dbContext.PatientProfiles
            .CountAsync(p => p.CreatedAt <= lastMonthEnd, cancellationToken);

        double totalPatientsChangePercent = 0;
        if (totalPatientsLastMonth > 0)
        {
            totalPatientsChangePercent = Math.Round(((totalPatients - totalPatientsLastMonth) / (double)totalPatientsLastMonth) * 100, 1);
        }

        // 2. New Patients This Month
        var newPatientsThisMonth = await _dbContext.PatientProfiles
            .CountAsync(p => p.CreatedAt >= currentMonthStart, cancellationToken);

        // New Patients Last Month
        var newPatientsLastMonth = await _dbContext.PatientProfiles
            .CountAsync(p => p.CreatedAt >= lastMonthStart && p.CreatedAt <= lastMonthEnd, cancellationToken);

        double newPatientsChangePercent = 0;
        if (newPatientsLastMonth > 0)
        {
            newPatientsChangePercent = Math.Round(((newPatientsThisMonth - newPatientsLastMonth) / (double)newPatientsLastMonth) * 100, 1);
        }

        // 3. Monthly Performance (Last 6 months)
        var monthlyPerformance = new List<MonthlyPerformanceDto>();

        for (int i = 5; i >= 0; i--)
        {
            var targetMonthStart = currentMonthStart.AddMonths(-i);
            var targetMonthEnd = targetMonthStart.AddMonths(1).AddTicks(-1);
            
            var startDateOnly = DateOnly.FromDateTime(targetMonthStart);
            var endDateOnly = DateOnly.FromDateTime(targetMonthEnd);

            var completedCount = await _dbContext.Bookings
                .CountAsync(b => b.Status == Clinic.Infrastructure.Entities.Enums.BookingStatus.COMPLETED 
                              && b.DoctorSchedule.Date >= startDateOnly 
                              && b.DoctorSchedule.Date <= endDateOnly, 
                            cancellationToken);

            var canceledCount = await _dbContext.Bookings
                .CountAsync(b => b.Status == Clinic.Infrastructure.Entities.Enums.BookingStatus.CANCELLED 
                              && b.DoctorSchedule.Date >= startDateOnly 
                              && b.DoctorSchedule.Date <= endDateOnly, 
                            cancellationToken);

            monthlyPerformance.Add(new MonthlyPerformanceDto(
                targetMonthStart.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                completedCount,
                canceledCount
            ));
        }

        var response = new AdminDashboardResponse(
            totalPatients,
            totalPatientsChangePercent,
            newPatientsThisMonth,
            newPatientsChangePercent,
            monthlyPerformance
        );

        return Ok(response);
    }
}

public record MonthlyPerformanceDto(string Month, int Completed, int Canceled);

public record AdminDashboardResponse(
    int TotalPatients,
    double TotalPatientsChangePercent,
    int NewPatientsThisMonth,
    double NewPatientsChangePercent,
    List<MonthlyPerformanceDto> MonthlyPerformance
);
