using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO;
using RentalManagementPlatformWebAPI.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class OccupancyController : ControllerBase
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public OccupancyController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        [HttpPost("GetOccupancy")]
        public async Task<IActionResult> GetOccupancy([FromBody] RevenueRequestDto req)
        {
            // TODO: 之後需從登入資訊取得 HostId
            int hostId = 47;

            List<int> roomIdsToQuery;

            // If no RoomId is provided, query all rooms for the host
            if (req.RoomIds == null || !req.RoomIds.Any())
            {
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId)
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }
            else
            {
                // Otherwise, use the provided RoomIds, but verify they belong to the host
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId && req.RoomIds.Contains(r.RoomId))
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }

            if (!roomIdsToQuery.Any())
            {
                return Ok(new object[0]);
            }

            // Get all rooms that are being queried to calculate total available nights
            var queriedRooms = await _context.RoomLists
                .Where(r => roomIdsToQuery.Contains(r.RoomId))
                .ToListAsync();

            // Filter bookings for occupancy calculation
            // Consider only Confirmed or Completed bookings for occupancy rate
            var occupancyBookings = _context.Bookings
                .Where(b => (b.Status == "Completed" || b.Status == "Confirmed"))
                .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                .Where(b => b.CheckIn.HasValue && b.CheckOut.HasValue &&
                            b.CheckIn.Value.Date <= req.EndDate.Date &&
                            b.CheckOut.Value.Date >= req.StartDate.Date);

            // Helper to get the start of the week (Monday)
            static DateTime GetStartOfWeek(DateTime dt)
            {
                int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
                return dt.AddDays(-1 * diff).Date;
            }

            switch (req.GroupBy?.ToLower())
            {
                case "day":
                default:
                    var allDaysInRange = new List<DateTime>();
                    var loopDateDay = req.StartDate.Date;
                    var finalDateDay = req.EndDate.Date;

                    while (loopDateDay <= finalDateDay)
                    {
                        allDaysInRange.Add(loopDateDay);
                        loopDateDay = loopDateDay.AddDays(1);
                    }

                    var dailyOccupancyData = new List<OccupancyDataPoint>();

                    foreach (var currentDay in allDaysInRange)
                    {
                        int occupiedRoomsCount = 0;
                        // Get bookings that are active on currentDay
                        var bookingsOnDay = await occupancyBookings
                            .Where(b => b.CheckIn.Value.Date <= currentDay && b.CheckOut.Value.Date > currentDay)
                            .ToListAsync();

                        // Count unique rooms occupied on this day
                        occupiedRoomsCount = bookingsOnDay.Select(b => b.RoomId).Distinct().Count();

                        // Total available rooms for this day is the count of queriedRooms
                        int totalAvailableRooms = queriedRooms.Count;

                        decimal occupancyRate = (totalAvailableRooms > 0)
                            ? (decimal)occupiedRoomsCount / totalAvailableRooms * 100m
                            : 0m;

                        dailyOccupancyData.Add(new OccupancyDataPoint
                        {
                            date = currentDay.ToString("yyyy-MM-dd"),
                            occupancyRate = occupancyRate
                        });
                    }
                    return Ok(dailyOccupancyData.OrderBy(d => d.date));

                case "week":
                    var allWeeksInRange = new List<DateTime>();
                    var loopDateWeek = GetStartOfWeek(req.StartDate);
                    var finalDateWeek = GetStartOfWeek(req.EndDate);

                    while (loopDateWeek <= finalDateWeek)
                    {
                        allWeeksInRange.Add(loopDateWeek);
                        loopDateWeek = loopDateWeek.AddDays(7);
                    }

                    var weeklyOccupancyData = new List<OccupancyDataPoint>();

                    foreach (var weekStart in allWeeksInRange)
                    {
                        int totalOccupiedNightsInWeek = 0;
                        int totalAvailableNightsInWeek = queriedRooms.Count * 7; // 7 days in a week

                        // Iterate each day in the week to calculate occupied nights
                        for (int i = 0; i < 7; i++)
                        {
                            var currentDay = weekStart.AddDays(i);
                            var bookingsOnDay = await occupancyBookings
                                .Where(b => b.CheckIn.Value.Date <= currentDay && b.CheckOut.Value.Date > currentDay)
                                .ToListAsync();
                            totalOccupiedNightsInWeek += bookingsOnDay.Select(b => b.RoomId).Distinct().Count();
                        }

                        decimal occupancyRate = (totalAvailableNightsInWeek > 0)
                            ? (decimal)totalOccupiedNightsInWeek / totalAvailableNightsInWeek * 100m
                            : 0m;

                        weeklyOccupancyData.Add(new OccupancyDataPoint
                        {
                            date = weekStart.ToString("yyyy-MM-dd"),
                            occupancyRate = occupancyRate
                        });
                    }
                    return Ok(weeklyOccupancyData.OrderBy(d => d.date));

                case "month":
                    var allMonthsInRange = new List<DateTime>();
                    var loopDateMonth = new DateTime(req.StartDate.Year, req.StartDate.Month, 1);
                    var finalDateMonth = new DateTime(req.EndDate.Year, req.EndDate.Month, 1);

                    while (loopDateMonth <= finalDateMonth)
                    {
                        allMonthsInRange.Add(loopDateMonth);
                        loopDateMonth = loopDateMonth.AddMonths(1);
                    }

                    var monthlyOccupancyData = new List<OccupancyDataPoint>();

                    foreach (var monthStart in allMonthsInRange)
                    {
                        int daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
                        int totalOccupiedNightsInMonth = 0;
                        int totalAvailableNightsInMonth = queriedRooms.Count * daysInMonth;

                        // Iterate each day in the month to calculate occupied nights
                        for (int i = 0; i < daysInMonth; i++)
                        {
                            var currentDay = monthStart.AddDays(i);
                            var bookingsOnDay = await occupancyBookings
                                .Where(b => b.CheckIn.Value.Date <= currentDay && b.CheckOut.Value.Date > currentDay)
                                .ToListAsync();
                            totalOccupiedNightsInMonth += bookingsOnDay.Select(b => b.RoomId).Distinct().Count();
                        }

                        decimal occupancyRate = (totalAvailableNightsInMonth > 0)
                            ? (decimal)totalOccupiedNightsInMonth / totalAvailableNightsInMonth * 100m
                            : 0m;

                        monthlyOccupancyData.Add(new OccupancyDataPoint
                        {
                            date = monthStart.ToString("yyyy-MM-dd"),
                            occupancyRate = occupancyRate
                        });
                    }
                    return Ok(monthlyOccupancyData.OrderBy(d => d.date));
            }
        }

        [HttpPost("GetOccupancyKpi")]
        public async Task<IActionResult> GetOccupancyKpi([FromBody] OccupancyKpiRequestDto req)
        {
            // TODO: 之後需從登入資訊取得 HostId
            int hostId = 47;

            List<int> roomIdsToQuery;

            // If no RoomId is provided, query all rooms for the host
            if (req.RoomIds == null || !req.RoomIds.Any())
            {
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId)
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }
            else
            {
                // Otherwise, use the provided RoomIds, but verify they belong to the host
                roomIdsToQuery = await _context.RoomLists
                    .Where(r => r.HostId == hostId && req.RoomIds.Contains(r.RoomId))
                    .Select(r => r.RoomId)
                    .ToListAsync();
            }

            if (!roomIdsToQuery.Any())
            {
                return Ok(new { occupancyRate = 0 });
            }

            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-req.Days);

            // Total available nights for the selected rooms over the period
            int totalAvailableNights = roomIdsToQuery.Count * req.Days;
            if (totalAvailableNights == 0)
            {
                return Ok(new { occupancyRate = 0 });
            }

            // Get bookings that overlap with the date range
            var bookings = await _context.Bookings
                .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                .Where(b => (b.Status == "Completed" || b.Status == "Confirmed"))
                .Where(b => b.CheckIn.Value < endDate && b.CheckOut.Value > startDate)
                .ToListAsync();

            double totalOccupiedNights = 0;

            foreach (var booking in bookings)
            {
                var bookingStart = booking.CheckIn.Value < startDate ? startDate : booking.CheckIn.Value;
                var bookingEnd = booking.CheckOut.Value > endDate ? endDate : booking.CheckOut.Value;
                totalOccupiedNights += (bookingEnd - bookingStart).TotalDays;
            }

            decimal occupancyRate = (totalAvailableNights > 0)
                ? ((decimal)totalOccupiedNights / totalAvailableNights) * 100
                : 0;

            return Ok(new { occupancyRate = Math.Round(occupancyRate, 2) });
                }
        
                [HttpPost("GetOccupancySourceAnalysis")]
                public async Task<IActionResult> GetOccupancySourceAnalysis([FromBody] AnalysisRequestDto req)
                {
                    // TODO: 之後需從登入資訊取得 HostId
                    int hostId = 47;
        
                    List<int> roomIdsToQuery;
        
                    if (req.RoomIds == null || !req.RoomIds.Any())
                    {
                        roomIdsToQuery = await _context.RoomLists
                            .Where(r => r.HostId == hostId)
                            .Select(r => r.RoomId)
                            .ToListAsync();
                    }
                    else
                    {
                        roomIdsToQuery = await _context.RoomLists
                            .Where(r => r.HostId == hostId && req.RoomIds.Contains(r.RoomId))
                            .Select(r => r.RoomId)
                            .ToListAsync();
                    }
        
                    if (!roomIdsToQuery.Any())
                    {
                        return Ok(new List<OccupancySourceDataPoint>());
                    }
        
                    var startDate = DateTime.Today.AddDays(-req.Days);
                    var endDate = DateTime.Today;
        
                    var analysis = await _context.Bookings
                        .Where(b => b.RoomId.HasValue && roomIdsToQuery.Contains(b.RoomId.Value))
                        .Where(b => b.Status == "Completed" || b.Status == "Confirmed")
                        .Where(b => b.CheckIn.HasValue && b.CheckIn.Value.Date >= startDate && b.CheckIn.Value.Date <= endDate)
                        .GroupBy(b => new { b.RoomId, b.Room.Title })
                        .Select(g => new OccupancySourceDataPoint
                        {
                            RoomId = g.Key.RoomId.Value,
                            RoomTitle = g.Key.Title,
                            BookingCount = g.Count()
                        })
                        .Where(r => r.BookingCount > 0)
                        .OrderByDescending(r => r.BookingCount)
                        .ToListAsync();
        
                    return Ok(analysis);
                }
            }
        }