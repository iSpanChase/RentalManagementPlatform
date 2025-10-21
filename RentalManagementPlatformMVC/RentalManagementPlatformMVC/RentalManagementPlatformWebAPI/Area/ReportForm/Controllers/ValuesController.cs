using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.Controllers
{
    [Area("ReportForm")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var data = new { Name = "Alice", Age = 18 };
            var jsonData = JsonSerializer.Serialize(data);
            return Content(jsonData, "application/json");
        }
    }
}
