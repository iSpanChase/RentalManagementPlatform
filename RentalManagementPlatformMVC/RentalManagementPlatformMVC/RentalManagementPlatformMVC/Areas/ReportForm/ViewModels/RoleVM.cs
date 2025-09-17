using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.ReportForm.ViewModels
{
    public class RoleVM
    {
        public int Id { private set; get; }
        public string Name {  private set; get; }

        public RoleVM(Role role)
        {
            Id = role.RoleId;
            Name = role.RoleName;
        }
    }
}
