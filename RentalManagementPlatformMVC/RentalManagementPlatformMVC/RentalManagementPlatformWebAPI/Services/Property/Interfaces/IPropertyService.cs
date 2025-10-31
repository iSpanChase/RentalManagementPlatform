using RentalManagementPlatformWebAPI.DTOs.Property;

namespace RentalManagementPlatformWebAPI.Services.Property.Interfaces
{
	public interface IPropertyService
	{
		Task<int> CreatePostAsync(PostCreateDto dto);
		Task<IEnumerable<PostListDto>> GetAllPostsAsync();
		Task<PostDetailDto>GetPostByIdAsync(int postId);
		Task<bool>UpdatePostAsync(PostUpdateDto dto);
		Task<bool>DeletePostAsync(int postId);
	}
}
