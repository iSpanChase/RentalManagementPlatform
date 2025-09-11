using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels
{
    public class DistrictVM
    {
        public int Id { private set; get; }
        public string Name {  private set; get; }

        public DistrictVM(District district)
        {
            Id = district.DistrictId;
            Name = district.DistrictName;
        }
    }
}
