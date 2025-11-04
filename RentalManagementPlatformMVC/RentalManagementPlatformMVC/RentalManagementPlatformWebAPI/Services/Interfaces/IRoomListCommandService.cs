using Microsoft.AspNetCore.Http;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    /// <summary>
    /// 定義房源相關的寫入操作指令服務介面。
    /// 負責處理房源資料的建立、更新、刪除以及圖片的添加等。
    /// </summary>
    public interface IRoomListCommandService
    {
        /// <summary>
        /// 建立一個新的房源。
        /// </summary>
        /// <param name="dto">包含新房源資料的 DTO。</param>
        /// <returns>建立成功的房源實體。</returns>
        Task<RoomList> CreateRoomAsync(CreateRoomRequestDto dto);

        /// <summary>
        /// 更新指定 ID 的房源資料。
        /// </summary>
        /// <param name="id">要更新的房源 ID。</param>
        /// <param name="dto">包含更新資料的 DTO。</param>
        Task UpdateRoomAsync(int id, UpdateRoomRequestDto dto);

        /// <summary>
        /// 刪除指定 ID 的房源 (通常是軟刪除)。
        /// </summary>
        /// <param name="id">要刪除的房源 ID。</param>
        Task DeleteRoomAsync(int id);

        /// <summary>
        /// 為指定房源添加一張照片，並處理照片的排序邏輯。
        /// </summary>
        /// <param name="roomPhoto">包含照片詳細資訊的 RoomPhoto 實體。</param>
        Task AddRoomPhotoAsync(RoomPhoto roomPhoto);

        /// <summary>
        /// 處理房源照片的上傳、建立資料庫紀錄並觸發索引更新的完整流程。
        /// </summary>
        /// <param name="roomId">房源 ID。</param>
        /// <param name="file">上傳的圖片檔案。</param>
        /// <param name="photoType">圖片類型 (例如: 'Cover', 'General')。</param>
        /// <returns>建立成功的 RoomPhoto 實體。</returns>
        Task<RoomPhoto> UploadAndAddPhotoAsync(int roomId, IFormFile file, string? photoType);

        /// <summary>
        /// 刪除指定的房源照片。
        /// </summary>
        /// <param name="photoId">要刪除的照片 ID。</param>
        /// <returns>如果找到並成功刪除則返回 true，否則返回 false。</returns>
        Task<bool> DeleteRoomPhotoAsync(int photoId);
    }
}