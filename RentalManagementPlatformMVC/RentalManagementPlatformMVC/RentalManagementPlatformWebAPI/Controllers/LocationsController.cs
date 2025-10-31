using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs.Location;
using RentalManagementPlatformWebAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public LocationsController(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a list of all cities.
        /// </summary>
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Cities
                .Select(c => new CityDto
                {
                    CityId = c.CityId,
                    CityName = c.CityName
                })
                .ToListAsync();
            
            return Ok(cities);
        }

        /// <summary>
        /// Gets a list of districts for a given city.
        /// </summary>
        /// <param name="cityId">The ID of the city.</param>
        [HttpGet("districts/{cityId}")]
        public async Task<IActionResult> GetDistricts(int cityId)
        {
            var districts = await _context.Districts
                .Where(d => d.CityId == cityId)
                .Select(d => new DistrictDto
                {
                    DistrictId = d.DistrictId,
                    DistrictName = d.DistrictName
                })
                .ToListAsync();

            return Ok(districts);
        }
    }
}
