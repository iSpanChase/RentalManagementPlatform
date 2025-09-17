namespace RentalManagementPlatformMVC.Areas.ReportForm.Anomaly
{
    public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems, int? RuleIdFilter = null)
    {
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
