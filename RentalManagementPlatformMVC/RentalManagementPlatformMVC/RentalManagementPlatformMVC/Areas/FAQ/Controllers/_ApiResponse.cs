namespace RentalManagementPlatformMVC.Areas.FAQ.Controllers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string? msg = null) => new() { Success = true, Data = data, Message = msg };
        public static ApiResponse<T> Fail(string msg) => new() { Success = false, Message = msg };
    }
}
