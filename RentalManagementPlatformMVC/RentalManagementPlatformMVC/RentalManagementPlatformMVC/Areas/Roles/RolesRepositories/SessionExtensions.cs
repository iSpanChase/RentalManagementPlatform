using System.Text.Json;

namespace RentalManagementPlatformMVC.Areas.Roles.RolesRepositories
{
	public static class SessionExtensions
	{
		public static void SetHashSetInt(this ISession session, string key, HashSet<int> set)
	=> session.SetString(key, JsonSerializer.Serialize(set));

		public static HashSet<int> GetHashSetInt(this ISession session, string key)
		{
			var json = session.GetString(key);
			return string.IsNullOrEmpty(json) ? new HashSet<int>() : JsonSerializer.Deserialize<HashSet<int>>(json)!;
		}
	}
}
