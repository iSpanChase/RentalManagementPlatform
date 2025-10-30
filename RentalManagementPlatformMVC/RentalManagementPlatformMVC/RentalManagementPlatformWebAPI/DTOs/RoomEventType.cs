namespace RentalManagementPlatformWebAPI.DTOs
{
    public enum RoomEventType
    {
        Created,
        Updated,
        Deleted,
        PhotoAdded // For when a photo is added, potentially changing cover image
    }
}
