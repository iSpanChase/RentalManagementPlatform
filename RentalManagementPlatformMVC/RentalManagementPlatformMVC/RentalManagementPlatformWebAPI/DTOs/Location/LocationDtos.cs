namespace RentalManagementPlatformWebAPI.DTOs.Location
{
    public class CityDto
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
    }

    public class DistrictDto
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
    }
}
