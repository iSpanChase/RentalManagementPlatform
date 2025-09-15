using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.ReportForm.Controllers;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Controllers
{
    /// <summary>
    /// 我的最愛（整頁報表快照）API
    /// - 路徑：/ReportForm/ReportForm/FavoritesList|Save|Get|Delete
    /// - 資料存放：
    ///   - UserFavoriteReport.ReportType   ← 儲存「我的最愛名稱」
    ///   - UserFavoriteReport.ReportParams ← 儲存「整頁卡片快照」JSON
    /// </summary>
    [ApiController]
    [Route("ReportForm/ReportForm/[action]")]
    public class FavoritesController : ControllerBase
    {
        private readonly RentalManagementPlatformSqlContext _db;
        private static readonly JsonSerializerOptions _jsonOpt = new JsonSerializerOptions
        {
            //屬性名稱會被轉換成 camelCase
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
            //序列化時，如果某屬性值是 null，就不會寫進 JSON。
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            //格式化 JSON，壓縮成單行，節省大小。
            WriteIndented = false
        };

        public FavoritesController(RentalManagementPlatformSqlContext db)
        {
            _db = db;
        }

        // 先用假資料取使用者 Id；你有登入系統時改成實際 UserId 來源（例如 HttpContext.User）
        private int GetCurrentUserId() => 1;

        /// <summary>列出目前使用者的所有最愛（下拉用）</summary>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<FavoriteListItem>>> FavoritesList()
        {
            var uid = GetCurrentUserId();

            var list = _db.UserFavoriteReports
                          .Where(x => x.UserId == uid)
                          .OrderByDescending(x => x.CreatedAt)
                          .Select(x => new FavoriteListItem
                          {
                              Id = x.FavoriteId,
                              Name = string.IsNullOrWhiteSpace(x.ReportType) ? "(未命名)" : x.ReportType,
                              CreatedAt = x.CreatedAt
                          })
                          .ToList();

            return Ok(list);
        }

        /// <summary>儲存目前頁面的卡片快照為一個「我的最愛」</summary>
        [HttpPost]
        public async Task<ActionResult> FavoritesSave([FromBody] FavoriteSaveRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Name))
                return BadRequest("請提供我的最愛名稱。");
            if (req?.Name.Length>50)
                return BadRequest("我的最愛名稱最多只能50個字元");

            var uid = GetCurrentUserId();

            // 序列化整頁快照
            var snapshot = new FavoriteSnapshot { Cards = req.Cards ?? new() };
            var json = JsonSerializer.Serialize(snapshot, _jsonOpt);

            var row = new UserFavoriteReport
            {
                UserId = uid,
                ReportType = req.Name,
                ReportParams = json,
                CreatedAt = DateTime.UtcNow
            };

            _db.UserFavoriteReports.Add(row);
            await _db.SaveChangesAsync();

            return Ok(new { id = row.FavoriteId });
        }

        /// <summary>取得指定我的最愛內容（載入整頁）</summary>
        [HttpPost]
        public async Task<ActionResult<FavoriteGetResponse>> FavoritesGet([FromBody] FavoriteGetRequest req)
        {
            var uid = GetCurrentUserId();

            var row = _db.UserFavoriteReports
                         .FirstOrDefault(x => x.FavoriteId == req.Id && x.UserId == uid);

            if (row == null) return NotFound("找不到該我的最愛。");

            var snap = ParseSnapshot(row.ReportParams);

            var resp = new FavoriteGetResponse
            {
                Id = row.FavoriteId,
                Name = row.ReportType ?? "(未命名)",
                Cards = snap.Cards,
                CreatedAt = row.CreatedAt
            };

            return Ok(resp);
        }

        /// <summary>刪除指定我的最愛</summary>
        [HttpPost]
        public async Task<ActionResult> FavoritesDelete([FromBody] FavoriteDeleteRequest req)
        {
            var uid = GetCurrentUserId();

            var row = _db.UserFavoriteReports
                         .FirstOrDefault(x => x.FavoriteId == req.Id && x.UserId == uid);

            if (row == null) return NotFound("找不到該我的最愛。");

            _db.UserFavoriteReports.Remove(row);
            await _db.SaveChangesAsync();

            return Ok(new { deleted = true });
        }

        // ===== helpers =====
        private static FavoriteSnapshot ParseSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new FavoriteSnapshot();

            try
            {
                var snap = JsonSerializer.Deserialize<FavoriteSnapshot>(json, _jsonOpt);
                return snap ?? new FavoriteSnapshot();
            }
            catch
            {
                // 舊資料或格式錯誤時回傳空快照，避免整頁崩潰
                return new FavoriteSnapshot();
            }
        }
    }
}
