using RentalManagementPlatformMVC.UserDTOs;

namespace RentalManagementPlatformMVC.Repositories
{
	public interface IUserService
	{
		Task<(IReadOnlyList<UserListItemDto> Items, int Total)> ListAsync(string? keyword, int page, int pageSize);
		Task<UserDetailDto> GetAsync(int userId);
		Task<int> CreateAsync(CreateUserDto dto);
		Task UpdateAsync(UpdateUserDto dto);
		Task DeleteAsync(int userId);
	}
}
