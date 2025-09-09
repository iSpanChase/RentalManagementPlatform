namespace RentalManagementPlatformMVC.DTOs
{
	/// <summary>
	/// 泛型分頁結果容器，用於封裝查詢結果與分頁相關的資訊。
	/// </summary>
	public class PagedResultDto<T>
    {
		// 目前頁面中的資料項目集合
		public IEnumerable<T> Items { get; set; } = new List<T>(); 
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
		// 計算總頁數
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); 
		public int TotalCount { get; set; }
	}
}
