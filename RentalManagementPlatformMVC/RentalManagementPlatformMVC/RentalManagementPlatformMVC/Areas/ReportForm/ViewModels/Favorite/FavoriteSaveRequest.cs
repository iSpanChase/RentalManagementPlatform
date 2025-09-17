using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    public class FavoriteSaveRequest
    {
        public string Name { get; set; } = "";
        public List<ReportCard> Cards { get; set; } = new();
    }
}
