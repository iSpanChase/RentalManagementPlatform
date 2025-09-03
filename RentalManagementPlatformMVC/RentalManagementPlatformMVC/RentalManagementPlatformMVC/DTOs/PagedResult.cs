namespace RentalManagementPlatformMVC.DTOs
{
	/// <summary>
	/// 泛型分頁結果容器，用於封裝查詢結果與分頁相關的資訊。
	/// </summary>
	/// <typeparam name="T">資料項目的型別。</typeparam>
	public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>(); // 目前頁面中的資料項目集合
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); // 計算總頁數
		public int TotalCount { get; set; }
	}
}
