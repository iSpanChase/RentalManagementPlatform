using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository
{
	public class PostRepository: IPostRepository
	{
		public readonly RentalManagementPlatformSqlContext _db;
		//建構子注入dbcontext
		public PostRepository(RentalManagementPlatformSqlContext db)
		{
			_db = db;
		}
		
		public IQueryable<Post> Query()
		{
			return _db.Posts.AsNoTracking();
		}
		//查詢Posts單筆文章
		public async Task<Post?> GetByIdAsync(int id)
		{
			return await _db.Posts.FirstOrDefaultAsync(p => p.PostsId == id);
		}
		//更新文章並寫入資料庫
		public async Task UpdateAsync(Post post)
		{
			_db.Posts.Update(post);
			await _db.SaveChangesAsync();
		}
		//統一提交資料庫變更
		public async Task SaveChangesAsync()
		{
			await _db.SaveChangesAsync();
		}
	}
}
