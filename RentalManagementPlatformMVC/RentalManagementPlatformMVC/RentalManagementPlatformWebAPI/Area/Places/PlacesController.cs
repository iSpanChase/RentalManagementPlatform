using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IConfiguration _config; // kept for backward compatibility if needed
        private readonly IPlacesService _places;

        public PlacesController(IConfiguration config, IPlacesService places)
        {
            _config = config;
            _places = places;
        }

        // GET: api/Places/search?q=台北101
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PlaceSuggestionDto>>> Search(string q, CancellationToken ct)
        {
            var list = await _places.SearchAsync(q, take: 10, ct);
            return Ok(list);
        }
    }
}
