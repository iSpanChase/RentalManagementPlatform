using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Management.Services
{
	public class PostQueryService : IPostQueryService
	{
		//注入資料庫存取層Repository
		private readonly IPostRepository _postrepo;
		private readonly IDistrictRepository _districtrepo;
		private readonly IUserReadRepository _userrepo;
		//注入建構式
		public PostQueryService(
			IPostRepository postrepo,
			IDistrictRepository districtrepo,
			IUserReadRepository userrepo)
		{
			_postrepo = postrepo;
			_districtrepo = districtrepo;
			_userrepo = userrepo;
		}

		//取得所有 Post 的 ViewModel
		//回傳PostQueryVm的集合,代表已經整合Post、User、District的資料
		//支援分頁與搜尋條件,回傳Data, int TotalCount
		public async Task<(List<PostQueryVm> Data, int TotalCount)> GetAllPostsVmAsync(
			int pageIndex = 1,
			int pageSize = 10,
			string? title = null,
			string? username = null,
			string? district = null,
			string? status = null,
			int? categoryId = null)
		{
			//防呆
			if (pageIndex < 1) pageIndex = 1;
			if (pageSize <= 0) pageSize = 10;

			var query =
				from p in _postrepo.Query()
				join u in _userrepo.Query() on p.UserId equals u.UserId into userJoin
				from u in userJoin.DefaultIfEmpty()
				join d in _districtrepo.Query() on p.RegionId equals d.DistrictId into districtJoin
				from d in districtJoin.DefaultIfEmpty()
				select new PostQueryVm
				{
					PostsId = p.PostsId,
					UserId = p.UserId,
					Name = u != null ? u.Name : "-",
					RegionId = p.RegionId,
					DistrictName = d != null ? d.DistrictName : "-",
					Title=p.Title,
					Content=p.Content,
					Status=p.Status,
					CreatedAt=p.CreatedAt,
					UpdatedAt=p.UpdatedAt,
				};
			//從posts、user、 districts資料表取得所有資料，- 使用 ToListAsync() 代表立即執行查詢並載入到記憶體中。
			//var posts = await _postrepo.Query().ToListAsync();
			//var users = await _userrepo.Query().ToListAsync();
			//var districts = await _districtrepo.Query().ToListAsync();

			//var query = posts.Select(p => new PostQueryVm
			//{
			//	PostsId = p.PostsId,
			//	UserId = p.UserId,
			//	Name = users.FirstOrDefault(u => u.UserId == p.UserId)?.Name ?? "-",
			//	RegionId = p.RegionId,
			//	DistrictName = districts.FirstOrDefault(d => d.DistrictId == p.RegionId)?.DistrictName ?? "-",
			//	Title = p.Title,
			//	Content = p.Content,
			//	Status = p.Status,
			//	CreatedAt = p.CreatedAt,
			//	UpdatedAt = p.UpdatedAt
			//}).AsQueryable();

			// 篩選條件
			if (!string.IsNullOrEmpty(title))
				query = query.Where(x => EF.Functions.Like(x.Title, $"%{title}%"));

			if (!string.IsNullOrEmpty(username))
				query = query.Where(x => EF.Functions.Like(x.Name, $"%{username}%"));

			if (!string.IsNullOrEmpty(district))
				query = query.Where(x => EF.Functions.Like(x.DistrictName, $"%{district}%"));

			if (!string.IsNullOrEmpty(status))
				query = query.Where(x => x.Status == status);

			if (categoryId.HasValue)
				query = query.Where(x => x.CategoryId == categoryId.Value);

			var totalCount=await query.CountAsync();
			
			var data=await query
				.OrderBy(x => x.PostsId)
				.Skip((pageIndex-1)*pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (data,totalCount);
		}

		public async Task UpdateStatusAsync(int postId, string newStatus)
		{
			var post = await _postrepo.GetByIdAsync(postId);
			if (post != null)
			{
				post.Status = newStatus;
				await _postrepo.UpdateAsync(post);
			}
		}
	}
}
