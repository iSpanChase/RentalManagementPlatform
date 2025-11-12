using System.Threading.Tasks;
using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services.AI.Interfaces
{
    public interface INaturalLanguageSearchService
    {
        Task<SearchQueryParameters> ParseNaturalLanguageQueryAsync(string naturalLanguageQuery);
        Task<IEnumerable<RoomListSearchDto>> ProcessNaturalLanguageSearchAsync(string naturalLanguageQuery);
    }

    public class SearchQueryParameters
    {
        public string? Query { get; set; }
        public string? Status { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusKm { get; set; }
        public bool SortByDistance { get; set; } = true;
        public PriceRange? PriceRange { get; set; }
        public int? MinGuests { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
    }

    public class PriceRange
    {
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
    }
}