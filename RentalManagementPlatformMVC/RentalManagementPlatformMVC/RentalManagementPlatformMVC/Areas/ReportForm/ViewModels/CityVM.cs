using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels
{
    public class CityVM
    {
        public int Id { private set; get; }
        public string Name {  private set; get; }

        public CityVM(City city)
        {
            Id = city.CityId;
            Name = city.CityName;
        }
    }
}
