using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces
{
	public interface IPostRepository
	{
		//回傳Post資料表的IQueryable延遲查詢
		IQueryable<Post> Query();
		//非同步取得單筆文章資料(Post?如果找不到資料會回傳null;)
		Task<Post?> GetByIdAsync(int id);
		//非同步更新文章資料
		Task UpdateAsync(Post post);
		//對資料庫的新增刪除編輯操作提交
		Task SaveChangesAsync();

	}
}
