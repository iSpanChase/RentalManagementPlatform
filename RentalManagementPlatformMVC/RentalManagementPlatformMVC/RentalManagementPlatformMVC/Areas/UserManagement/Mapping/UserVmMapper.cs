using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;

namespace RentalManagementPlatformMVC.Areas.UserManagement.Mapping
{
	public static class UserVmMapper
	{
		// ListItem DTO -> VM
		public static UserListItemVm ToVm(this UserDTOs.UserListItemDto d) => new()
		{
			UserId = d.UserId,
			Username = d.Username,
			Name = d.Name,
			Email = d.Email,
			CreatedAt = d.CreatedAt
		};

		// Create VM -> DTO
		public static CreateUserDto ToDto(this UserCreateVm vm) => new()
		{
			Username = vm.Username,
			Email = vm.Email,
			Name = vm.Name,
			AutoSubscribe = vm.AutoSubscribe,
			PasswordHash = vm.PasswordHash,
			Gender = vm.Gender,
			BirthDate = vm.BirthDate,
			Phone = vm.Phone,
			Address = vm.Address,
			Point = vm.Point,
			Isverified = vm.Isverified,
			ProfileImageurl = vm.ProfileImageurl
		};

		// Detail DTO -> Edit VM
		public static UserEditVm ToEditVm(this UserDTOs.UserDetailDto d) => new()
		{
			UserId = d.UserId,
			Username = d.Username,
			Email = d.Email,
			Name = d.Name,
			AutoSubscribe = d.AutoSubscribe,
			Gender = d.Gender,
			BirthDate = d.BirthDate,
			Phone = d.Phone,
			Address = d.Address,
			Point = d.Point,
			Isverified = d.Isverified,
			ProfileImageurl = d.ProfileImageurl
		};

		// Edit VM -> Update DTO
		public static UserDTOs.UpdateUserDto ToDto(this UserEditVm vm) => new()
		{
			UserId = vm.UserId,
			Email = vm.Email,
			Name = vm.Name,
			AutoSubscribe = vm.AutoSubscribe,
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
