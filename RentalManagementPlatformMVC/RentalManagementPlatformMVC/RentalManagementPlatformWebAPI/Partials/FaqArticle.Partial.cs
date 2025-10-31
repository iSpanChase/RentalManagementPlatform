using System.ComponentModel.DataAnnotations.Schema;

namespace RentalManagementPlatformWebAPI.Models
{
    public partial class FaqArticle
    {
        //// 明確宣告外鍵，對應 DB 欄位 category_id
        //[Column("category_id")]
        //public int CategoryId { get; set; }

        // 導覽屬性
        [ForeignKey(nameof(CategoryId))]
        public virtual FaqCategory? Category { get; set; }

        public virtual ICollection<FaqFeedback> FaqFeedbacks { get; set; } = new List<FaqFeedback>();
    }
}
