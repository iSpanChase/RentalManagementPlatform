using RentalManagementPlatformMVC.Areas.Management.DTOS;
using RentalManagementPlatformMVC.Areas.Management.ViewModels;
using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;

namespace RentalManagementPlatformMVC.Areas.Management.Services.Interfaces
{
	public interface IPostQueryService
	{
		//查詢文章列表,多條件篩選
		Task<(List<PostQueryVm> Data, int TotalCount)> GetAllPostsVmAsync(int pageInedx=1,int pageSize=10,string? title = null, string? username = null, string? district = null, string? status = null, int? categoryId = null);
		//更新Status的狀態
		Task UpdateStatusAsync(int postId, string newStatus);
	}
}
