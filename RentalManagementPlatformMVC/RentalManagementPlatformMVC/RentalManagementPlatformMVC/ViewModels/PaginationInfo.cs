namespace RentalManagementPlatformMVC.ViewModels
{
    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => CurrentPage > 1; 
        public bool HasNextPage => CurrentPage < TotalPages; 
        public int StartItem => (CurrentPage - 1) * PageSize + 1; 
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }
}
