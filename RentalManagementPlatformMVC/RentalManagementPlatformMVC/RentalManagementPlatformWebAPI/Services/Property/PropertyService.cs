using RentalManagementPlatformWebAPI.DTOs.Property;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Property.Interfaces;
using RentalManagementPlatformWebAPI.Services.Property.Interfaces;

namespace RentalManagementPlatformWebAPI.Services.Property
{
	public class PropertyService : IPropertyService
	{
		private readonly IPropertyRepository _repository;
		public PropertyService(IPropertyRepository repository)
		{
			_repository = repository;
		}

		public async Task<int> CreatePostAsync(PostCreateDto dto)
		{
			var post = new Post
			{
				UserId = dto.UserId,
				RegionId = dto.RegionId,
				Title = dto.Title,
				ContactName = dto.ContactName,
				Content = dto.Content,
				Address = dto.Address,
				ContactPhone = dto.ContactPhone,
				ContactEmail = dto.ContactEmail,
				ContactNote = dto.ContactNote,
				ProposedPrice = dto.ProposedPrice,
				PublishAt = dto.PublishAt,
				ExpireAt = dto.ExpireAt,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			};
			return await _repository.CreatePostAsync(post);
		}

		public async Task<bool> DeletePostAsync(int postId)
		{
			return await _repository.DeletePostAsync(postId);
		}

		public async Task<IEnumerable<PostListDto>> GetAllPostsAsync()
		{
			var posts = await _repository.GetAllPostsAsync();
			return posts.Select(p => new PostListDto
			{
				PostsId = p.PostsId,
				Title = p.Title,
				ContactName = p.ContactName,
				Status = p.Status,
				Views = p.Views ?? 0,
				PublishAt = p.PublishAt,
				ExpireAt = p.ExpireAt
			});

		}

		public async Task<PostDetailDto?> GetPostByIdAsync(int postId)
		{
			var post = await _repository.GetPostByIdAsync(postId);
			if (post == null)
			{
				return null;
			}
			return new PostDetailDto
			{
				PostsId = post.PostsId,
				UserId = post.UserId,
				RegionId = post.RegionId,
				Title = post.Title,
				ContactName = post.ContactName,
				Content = post.Content,
				Address = post.Address,
				ContactPhone = post.ContactPhone,
				ContactEmail = post.ContactEmail,
				ContactNote = post.ContactNote,
				ProposedPrice = post.ProposedPrice,
				Status = post.Status,
				Views = post.Views ?? 0,
				PublishAt = post.PublishAt,
				ExpireAt = post.ExpireAt,
				CreatedAt = post.CreatedAt,
				UpdatedAt = post.UpdatedAt,
				DeletedAt = post.DeletedAt
			};
		}

		public async Task<bool> UpdatePostAsync(PostUpdateDto dto)
		{
			var post = await _repository.GetPostByIdAsync(dto.PostsId);
			if (post == null)
			{
				return false;
			}
			post.Title = dto.Title;
			post.ContactName = dto.ContactName;
			post.Content = dto.Content;
			post.Address = dto.Address;
			post.ContactPhone = dto.ContactPhone;
			post.ContactEmail = dto.ContactEmail;
			post.ContactNote = dto.ContactNote;
			post.ProposedPrice = dto.ProposedPrice;
			post.ExpireAt = dto.ExpireAt;
			post.Status = dto.Status;
			post.UpdatedAt = DateTime.Now;

			return await _repository.UpdatePostAsync(post);
		}
	}
}
