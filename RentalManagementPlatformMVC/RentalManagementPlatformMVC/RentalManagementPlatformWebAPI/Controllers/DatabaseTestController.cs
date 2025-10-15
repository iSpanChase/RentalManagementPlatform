using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using System;

[ApiController]
[Route("api/[controller]")]
public class DatabaseTestController : ControllerBase
{
	private readonly RentalManagementPlatformSqlContext _context; 

	public DatabaseTestController(RentalManagementPlatformSqlContext context)
	{
		_context = context;
	}

	[HttpGet("test-connection")]
	public async Task<IActionResult> TestConnection()
	{
		try
		{
			// 測試資料庫連線
			var canConnect = await _context.Database.CanConnectAsync();

			if (canConnect)
			{
				return Ok(new
				{
					success = true,
					message = "✓ 資料庫連線成功！",
					database = _context.Database.GetDbConnection().Database,
					server = _context.Database.GetDbConnection().DataSource,
					timestamp = DateTime.Now
				});
			}
			else
			{
				return StatusCode(500, new
				{
					success = false,
					message = "✗ 無法連線到資料庫"
				});
			}
		}
		catch (Exception ex)
		{
			return StatusCode(500, new
			{
				success = false,
				message = "✗ 資料庫連線失敗",
				error = ex.Message,
				innerError = ex.InnerException?.Message
			});
		}
	}
}