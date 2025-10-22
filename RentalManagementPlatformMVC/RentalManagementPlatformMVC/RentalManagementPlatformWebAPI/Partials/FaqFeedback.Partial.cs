namespace RentalManagementPlatformWebAPI.Models
{
    public partial class FaqFeedback
    {
        public virtual FaqArticle? Article { get; set; }   // ← 補上
    }
}