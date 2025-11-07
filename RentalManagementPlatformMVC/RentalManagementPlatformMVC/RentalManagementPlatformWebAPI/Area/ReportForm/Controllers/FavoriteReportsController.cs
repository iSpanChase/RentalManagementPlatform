
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO;
using RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Report;
using RentalManagementPlatformWebAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class FavoriteReportsController : ApiControllerBase
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public FavoriteReportsController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        // GET: api/ReportForm/FavoriteReports
        [HttpGet]
        [Authorize(Policy = "Favorites.View")]
        public async Task<IActionResult> GetFavoriteReports()
        {
            int userId = CurrentUserId;

            var favorites = await _context.UserFavoriteReports
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FavoriteReportDto { Id = f.FavoriteId, Name = f.ReportType })
                .ToListAsync();

            return Ok(favorites);
        }

        // GET: api/ReportForm/FavoriteReports/5
        [HttpGet("{id}")]
        [Authorize(Policy = "Favorites.View")]
        public async Task<IActionResult> GetFavoriteReport(int id)
        {
            int userId = CurrentUserId;

            var favorite = await _context.UserFavoriteReports
                .FirstOrDefaultAsync(f => f.FavoriteId == id && f.UserId == userId);

            if (favorite == null)
            {
                return NotFound();
            }

            var favoriteDto = new FavoriteReportDetailDto
            {
                Id = favorite.FavoriteId,
                Name = favorite.ReportType,
                Content = favorite.ReportParams,
                CreatedAt = favorite.CreatedAt ?? DateTime.MinValue
            };

            return Ok(favoriteDto);
        }

        // POST: api/ReportForm/FavoriteReports
        [HttpPost]
        [Authorize(Policy = "Favorites.Create")]
        public async Task<IActionResult> CreateFavoriteReport([FromBody] CreateFavoriteReportDto createDto)
        {
            if (createDto == null || string.IsNullOrWhiteSpace(createDto.Name) || string.IsNullOrWhiteSpace(createDto.Content))
            {
                return BadRequest("Name and content cannot be empty.");
            }
            
            if (createDto.Name.Length > 20)
            {
                return BadRequest("Name cannot exceed 20 characters.");
            }

            int userId = CurrentUserId;

            var newFavorite = new UserFavoriteReport
            {
                UserId = userId,
                ReportType = createDto.Name,
                ReportParams = createDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFavoriteReports.Add(newFavorite);
            await _context.SaveChangesAsync();

            var resultDto = new FavoriteReportDto
            {
                Id = newFavorite.FavoriteId,
                Name = newFavorite.ReportType
            };

            return CreatedAtAction(nameof(GetFavoriteReport), new { id = newFavorite.FavoriteId }, resultDto);
        }

        // DELETE: api/ReportForm/FavoriteReports/5
        [Authorize(Policy = "Favorites.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavoriteReport(int id)
        {
            int userId = CurrentUserId;

            var favorite = await _context.UserFavoriteReports
                .FirstOrDefaultAsync(f => f.FavoriteId == id && f.UserId == userId);

            if (favorite == null)
            {
                return NotFound();
            }

            _context.UserFavoriteReports.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
