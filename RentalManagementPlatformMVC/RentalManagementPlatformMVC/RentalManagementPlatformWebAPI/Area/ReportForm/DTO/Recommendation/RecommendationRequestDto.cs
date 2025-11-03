using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformWebAPI.Area.ReportForm.DTO.Recommendation
{
    public class RecommendationRequestDto
    {
        public int? GuestId { get; set; }

        [Range(1, 100)]
        public int TopN { get; set; } = 20;

        [Range(1, 100)]
        public int DisplayM { get; set; } = 10;
    }
}
