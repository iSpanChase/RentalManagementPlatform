using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    /// <summary>
    /// Read-only place search (geocoding by text). Converts a place/landmark name (e.g., 台北101)
    /// into suggested coordinates for downstream nearby-room queries. Does not mutate any data.
    /// </summary>
    public interface IPlacesService
    {
        /// <summary>
        /// Search attractions/landmarks by text (biased to Taipei area) and return up to <paramref name="take"/> suggestions.
        /// This method is optimized for UI place suggestions (e.g., 台北 → 台北101)。
        /// </summary>
        /// <param name="query">Place or landmark name (any language). Must be non-empty after trimming.</param>
        /// <param name="take">Max number of suggestions to return. Typical range: 1-10. Default 5.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Zero or more <see cref="PlaceSuggestionDto"/> items ordered by provider relevance.</returns>
        Task<IReadOnlyList<PlaceSuggestionDto>> SearchAsync(string query, int take = 5, CancellationToken ct = default);

        /// <summary>
        /// Broad place search (addresses/administrative areas). Uses Text Search first, then Geocoding as fallback.
        /// Intended for AI function-calling robustness when user inputs general place names.
        /// </summary>
        Task<IReadOnlyList<PlaceSuggestionDto>> SearchBroadAsync(string query, int take = 5, CancellationToken ct = default);
    }
}
