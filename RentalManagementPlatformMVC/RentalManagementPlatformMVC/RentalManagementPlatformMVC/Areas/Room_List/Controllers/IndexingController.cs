
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Services;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Areas.Room_List.Controllers
{
    [Area("Room_List")]
    public class IndexingController : Controller
    {
        private readonly MeilisearchService _meilisearchService;

        public IndexingController(MeilisearchService meilisearchService)
        {
            _meilisearchService = meilisearchService;
        }

        /// <summary>
        /// Triggers the re-indexing of all room lists into Meilisearch.
        /// </summary>
        /// <returns>A content result indicating the process has started.</returns>
        public async Task<IActionResult> Index()
        {
            // This is intentionally not awaited in the request context
            // because indexing can be a long-running process.
            // We trigger it and let it run in the background.
            await _meilisearchService.IndexAllRoomListsAsync();

            string message = "Meilisearch indexing process has been started. <br>" +
                             "Please check your application's console logs to see the progress. <br>" +
                             "It may take a few moments for the data to become searchable. <br><br>" +
                             "<a href=\"Room_List/Search\">Go back to Search</a>";

            return Content(message, "text/html");
        }
    }
}
