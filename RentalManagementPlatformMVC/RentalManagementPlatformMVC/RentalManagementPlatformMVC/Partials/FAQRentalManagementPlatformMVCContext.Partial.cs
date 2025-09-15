using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Models
{
    public partial class RentalManagementPlatformSqlContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // ✅ 自關聯（Category ↔ Category）
            modelBuilder.Entity<FaqCategory>(entity =>
            {
                // 主鍵名稱沿用你的 Scaffold 欄位（FaqCategoriesId）
                entity.HasKey(e => e.FaqCategoriesId);

                entity.HasOne(e => e.Parent)            // 我有一個父
                      .WithMany(p => p.Children)        // 父有很多子
                      .HasForeignKey(e => e.ParentId)   // FK 欄位
                      .OnDelete(DeleteBehavior.NoAction) // 刪父不連動刪子，避免一串刪光
                      .HasConstraintName("FK_FaqCategory_Parent"); // 可選的 FK 名稱
            });

            // FaqArticle <-> FaqCategory
            modelBuilder.Entity<FaqArticle>(entity =>
            {
                // 明確指定欄位名稱
                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                // 一(分類)對多(文章)
                entity.HasOne(a => a.Category)
                      .WithMany(c => c.FaqArticles)
                      .HasForeignKey(a => a.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .HasConstraintName("FK_FAQ_ARTICLES_FAQ_CATEGORIES");
            });
            modelBuilder.Entity<FaqFeedback>(entity =>
            {
                entity.HasOne(f => f.Article)
                      .WithMany(a => a.FaqFeedbacks)
                      .HasForeignKey(f => f.ArticleId)
                      .OnDelete(DeleteBehavior.Cascade)           // 刪文章時連動刪回饋（你DB就是這樣）
                      .HasConstraintName("FK_FaqFeedback_Article");
            });
        }
    }
}
