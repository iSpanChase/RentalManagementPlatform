using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;

namespace RentalManagementPlatformMVC.Areas.UserManagement.Mapping
{
	/// <summary>
	/// 使用者 ViewModel 與 DTO 的映射工具。
	/// </summary>
	public static class UserVmMapper
	{
		/// <summary>
		/// 將清單 DTO 轉為清單項目 ViewModel。
		/// </summary>
		public static UserListItemVm ToVm(this UserDTOs.UserListItemDto d) => new()
		{
			UserId = d.UserId,
			Username = d.Username,
			Name = d.Name,
			Email = d.Email,
			CreatedAt = d.CreatedAt
		};

		/// <summary>
		/// 將 Create ViewModel 轉為 Create DTO。
		/// </summary>
		public static CreateUserDto ToDto(this UserCreateVm vm) => new()
		{
			Username = vm.Username,
			Email = vm.Email,
			Name = vm.Name,
			PasswordHash = vm.PasswordHash,
			Gender = vm.Gender,
			BirthDate = vm.BirthDate,
			Phone = vm.Phone,
			Address = vm.Address,
			Point = vm.Point,
			Isverified = vm.Isverified,
			ProfileImageurl = vm.ProfileImageurl
		};

		/// <summary>
		/// 將明細 DTO 轉為 Edit ViewModel。
		/// </summary>
		public static UserEditVm ToEditVm(this UserDTOs.UserDetailDto d) => new()
		{
			UserId = d.UserId,
			Username = d.Username,
			Email = d.Email,
			Name = d.Name,
			Gender = d.Gender,
			BirthDate = d.BirthDate,
			Phone = d.Phone,
			Address = d.Address,
			Point = d.Point,
			Isverified = d.Isverified,
			ProfileImageurl = d.ProfileImageurl
		};

		/// <summary>
		/// 將 Edit ViewModel 轉為 Update DTO。
		/// </summary>
		public static UserDTOs.UpdateUserDto ToDto(this UserEditVm vm) => new()
		{
			UserId = vm.UserId,
			Email = vm.Email,
			Name = vm.Name,
			Gender = vm.Gender,
			BirthDate = vm.BirthDate,
			Phone = vm.Phone,
			Address = vm.Address,
			Point = vm.Point,
			Isverified = vm.Isverified,
			ProfileImageurl = vm.ProfileImageurl
		};
	}
}
