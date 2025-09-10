namespace RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories
{
	/// <summary>
	/// 工作單元，負責提交資料變更的交易界線。
	/// </summary>
	public interface IUnitOfWork
	{
		/// <summary>
		/// 非同步提交變更。
		/// </summary>
		/// <param name="ct">取消Token。</param>
		/// <returns>受影響的筆數。</returns>
		Task<int> SaveChangesAsync(CancellationToken ct = default);
	}
}
