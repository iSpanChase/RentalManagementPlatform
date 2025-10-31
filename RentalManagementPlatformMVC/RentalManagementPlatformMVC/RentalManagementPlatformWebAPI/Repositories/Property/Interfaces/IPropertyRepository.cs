
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Property.Interfaces
{
	//公告資料的存取介面
	public interface IPropertyRepository
	{
		//新增公告,回傳新增公告的Id
		Task<int> CreatePostAsync(Post post);

		//根據Id取得公告資料
		Task<Post?> GetPostByIdAsync(int postId);

		//取得所有公告資料
		Task<IEnumerable<Post>> GetAllPostsAsync();

		//更新公告內容
		Task<bool> UpdatePostAsync(Post post);

		//根據Id刪除公告資料
		Task<bool> DeletePostAsync(int postId);
	}
}
