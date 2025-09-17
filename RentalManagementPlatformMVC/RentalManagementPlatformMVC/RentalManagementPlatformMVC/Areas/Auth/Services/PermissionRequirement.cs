using Microsoft.AspNetCore.Authorization;

namespace RentalManagementPlatformMVC.Areas.Auth.Services
{
		/// <summary>
		/// 權限需求：可設定多個權限代碼 + 比對模式
		/// </summary>
		public sealed class PermissionRequirement : IAuthorizationRequirement
		{
			public IReadOnlyList<string> Codes { get; }
			public PermissionMatchMode MatchMode { get; }

			public PermissionRequirement(IEnumerable<string> codes, PermissionMatchMode matchMode = PermissionMatchMode.Any)
			{
				Codes = codes.Select(s => s.Trim())
							 .Where(s => !string.IsNullOrWhiteSpace(s))
							 .Distinct(StringComparer.OrdinalIgnoreCase)
							 .ToArray();
				MatchMode = matchMode;
			}

			// 單碼 overload，方便呼叫
			public PermissionRequirement(string code, PermissionMatchMode matchMode = PermissionMatchMode.Any)
				: this(new[] { code }, matchMode) { }
		}

		public enum PermissionMatchMode
		{
			Any = 0, // 只要有其中一個
			All = 1  // 必須全部符合
		}
}
