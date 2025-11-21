namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 角色資料的對外傳輸模型：
    /// - Id：角色主鍵
    /// - Name：前端顯示名稱（可能是 RoleName 或 RoleCode）
    /// - Code：實際角色代碼（例如 ADMIN / TENANT / HOST）
    /// </summary>
    public record RoleDto(int Id, string Name, string Code);

    /// <summary>
    /// 權限資料的對外傳輸模型：
    /// - Id：權限主鍵
    /// - Code：權限代碼（PermCode），例如 Admin.ApproveOperator
    /// - DisplayName：顯示名稱（給前端 UI 使用）
    /// - Category：分類 / 模組名稱（用於前端分組顯示）
    /// </summary>
    public record PermissionDto(int Id, string Code, string DisplayName, string Category);

    /// <summary>
    /// 用於「指派權限給角色」時接收前端提交的資料：
    /// - PermissionIds：被勾選的權限 Id 清單
    ///   通常會搭配路由中的 roleId 一起使用
    /// </summary>
    public class AssignPermissionDto
    {
        /// <summary>
        /// 要指派或更新到角色身上的權限 Id 清單
        /// </summary>
        public List<int> PermissionIds { get; set; } = new();
    }
}
