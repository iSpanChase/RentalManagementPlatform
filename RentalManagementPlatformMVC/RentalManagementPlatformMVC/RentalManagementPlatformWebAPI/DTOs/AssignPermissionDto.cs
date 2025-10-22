namespace RentalManagementPlatformWebAPI.DTOs
{

	public record RoleDto(int Id, string Name, string Code);
	public record PermissionDto(int Id, string Code, string DisplayName, string Category);
	public class AssignPermissionDto
	{
		public List<int> PermissionIds { get; set; } = new();
	}
}
