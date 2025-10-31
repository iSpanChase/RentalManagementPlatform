using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Property.Interfaces;

namespace RentalManagementPlatformWebAPI.Repositories.Property
{
	public class PropertyRepository : IPropertyRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;
		public PropertyRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		public async Task<int> CreatePostAsync(Post post)
		{
			_context.Posts.Add(post);
			await _context.SaveChangesAsync();
			return post.PostsId;
		}

		public async Task<bool> DeletePostAsync(int postId)
		{
			var post = await _context.Posts.FindAsync(postId);
			if (post == null) 
			{
				return false;
			}
			post.DeletedAt = DateTime.Now;
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<IEnumerable<Post>> GetAllPostsAsync()
		{
			return await _context.Posts
				.Where(p => p.DeletedAt == null)
				.ToListAsync();
		}

		public async Task<Post?> GetPostByIdAsync(int postId)
		{
			return await _context.Posts
				.FirstOrDefaultAsync(p => p.PostsId == postId && p.DeletedAt == null);
		}

		public async Task<bool> UpdatePostAsync(Post post)
		{
			_context.Posts.Update(post);
			return await _context.SaveChangesAsync() > 0;
		}
	}
}
