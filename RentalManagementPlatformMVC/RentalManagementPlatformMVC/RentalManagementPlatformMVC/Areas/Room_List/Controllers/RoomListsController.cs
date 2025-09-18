using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Room_List.Models;
using RentalManagementPlatformMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace RentalManagementPlatformMVC.Areas.Room_List.Controllers
{
    [Area("Room_List")]
    [Authorize]
    public class RoomListsController : Controller
    {
        private readonly IRoomListQueryService _queryService;
        private readonly IRoomListCommandService _commandService;

        public RoomListsController(IRoomListQueryService queryService, IRoomListCommandService commandService)
        {
            _queryService = queryService;
            _commandService = commandService;
        }

        // GET: Room_List/RoomLists
        public async Task<IActionResult> Index()
        {
            var roomSummaries = await _queryService.GetRoomSummariesAsync();
            return View(roomSummaries);
        }

        // GET: Room_List/RoomLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomDetails = await _queryService.GetRoomDetailsAsync(id.Value);
            if (roomDetails == null)
            {
                return NotFound();
            }

            return View(roomDetails);
        }

        // GET: Room_List/RoomLists/Create
        public IActionResult Create()
        {
            var viewModel = new RoomInputViewModel();
            return View(viewModel);
        }

        // POST: Room_List/RoomLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomInputViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                await _commandService.CreateRoomAsync(viewModel);
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Room_List/RoomLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _queryService.GetRoomForEditAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        // POST: Room_List/RoomLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomInputViewModel viewModel)
        {
            if (id != viewModel.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _commandService.UpdateRoomAsync(id, viewModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _queryService.RoomListExistsAsync(viewModel.RoomId ?? 0))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Room_List/RoomLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomSummary = await _queryService.GetRoomSummaryForDeleteAsync(id.Value);
            if (roomSummary == null)
            {
                return NotFound();
            }

            return View(roomSummary);
        }

        // POST: Room_List/RoomLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _commandService.DeleteRoomAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}