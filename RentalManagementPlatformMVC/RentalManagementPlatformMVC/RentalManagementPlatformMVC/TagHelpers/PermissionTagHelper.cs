using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RentalManagementPlatformMVC.TagHelpers
{
	/// <summary>
	/// Razor 用法：asp-permission="Users.Create,Users.Edit"
	/// </summary>
	[HtmlTargetElement(Attributes = "asp-permission")]
	public sealed class PermissionTagHelper : TagHelper
	{
		private readonly IHttpContextAccessor _http;
		private static readonly StringComparer Cmp = StringComparer.OrdinalIgnoreCase;

		public PermissionTagHelper(IHttpContextAccessor http)
		{
			_http = http;
		}

		/// <summary>
		/// 權限代碼清單（逗號分隔）
		/// </summary>
		[HtmlAttributeName("asp-permission")]
		public string? Permission { get; set; }

		/// <summary>
		/// 比對模式：Any / All（預設 Any）
		/// </summary>
		[HtmlAttributeName("asp-permission-mode")]
		public string Mode { get; set; } = "Any";

		public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var user = _http.HttpContext?.User;
			if (user == null || string.IsNullOrWhiteSpace(Permission))
			{
				output.SuppressOutput();
				return Task.CompletedTask;
			}

			var codes = Permission.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
								  .Distinct(Cmp)
								  .ToArray();
			if (codes.Length == 0)
			{
				output.SuppressOutput();
				return Task.CompletedTask;
			}

			var userPerms = user.Claims
								.Where(c => Cmp.Equals(c.Type, "permission"))
								.Select(c => c.Value)
								.ToHashSet(Cmp);

			bool requireAll = string.Equals(Mode, "All", StringComparison.OrdinalIgnoreCase);
			bool ok = requireAll
				? codes.All(code => userPerms.Contains(code))
				: codes.Any(code => userPerms.Contains(code));

			if (!ok)
				output.SuppressOutput();

			return Task.CompletedTask;
		}
	}
}
