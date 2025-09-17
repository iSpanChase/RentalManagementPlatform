namespace RentalManagementPlatformMVC.Areas.ReportForm.Controllers
{
    public class FavoriteGetResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<ReportCard> Cards { get; set; } = new();
        public DateTime? CreatedAt { get; set; }
    }
}
