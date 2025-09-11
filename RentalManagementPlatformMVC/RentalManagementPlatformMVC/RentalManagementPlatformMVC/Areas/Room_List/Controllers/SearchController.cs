using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Services;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Areas.Room_List.Controllers
{
    [Area("Room_List")]
    public class SearchController : Controller
    {
        private readonly MeilisearchService _meilisearchService;

        public SearchController(MeilisearchService meilisearchService)
        {
            _meilisearchService = meilisearchService;
        }

        /// <summary>
        /// Performs a search using the Meilisearch service and displays the results.
        /// </summary>
        /// <param name="query">The search term from the user.</param>
        /// <returns>A view with the search results.</returns>
        public async Task<IActionResult> Index(string query)
        {
            ViewData["CurrentQuery"] = query;
            var results = await _meilisearchService.SearchAsync(query);
            return View(results);
        }
    }
}