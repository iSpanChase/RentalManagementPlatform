using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models
{
    // ✅ 不動你原本的 FaqCategory.cs，這裡用 partial 只補導覽屬性
    public partial class FaqCategory
    {
        // 父節點（ParentId 指向的那一筆）
        public virtual FaqCategory? Parent { get; set; }

        // 子節點集合（被我當成父的那些）
        public virtual ICollection<FaqCategory> Children { get; set; } = new List<FaqCategory>();

        // 若你有 FAQ 文章表（例如 FaqArticle），建議也補上這個集合（沒有就刪掉）
        public virtual ICollection<FaqArticle> FaqArticles { get; set; } = new List<FaqArticle>();
    }
}
