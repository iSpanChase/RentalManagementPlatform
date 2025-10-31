import apiClient from './axiosInstance';

// Define an interface for the successful upload response
interface UploadResponse {
  message: string;
  objectKey: string;
}

/**
 * Uploads a photo for a specific room.
 * @param {number} roomId The ID of the room.
 * @param {File} imageFile The image file to upload.
 * @param {string} [photoType='General'] The type of the photo.
 * @returns {Promise<UploadResponse>} The response from the server.
 */
export const uploadRoomPhoto = async (roomId: number, imageFile: File, photoType: string = 'General'): Promise<UploadResponse> => {
  const formData = new FormData();
  formData.append('ImageFile', imageFile);
  formData.append('PhotoType', photoType);

  const response = await apiClient.post<UploadResponse>(`/Rooms/${roomId}/upload-image`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  return response.data;
};

/**
 * Deletes a specific photo by its ID.
 * @param {number} photoId The ID of the photo to delete.
 * @returns {Promise<void>}
 */
export const deleteRoomPhoto = async (photoId: number): Promise<void> => {
  await apiClient.delete(`/Photos/${photoId}`);
};
