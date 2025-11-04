import apiClient from './axiosInstance';

// Define an interface for the room data payload
interface RoomData {
  title: string;
  description: string;
  pricePerNight: number;
  maxGuests: number;
  hostId?: number;
  cityId?: number;
  districtId?: number;
  street?: string;
}

// Define an interface for the response of the createRoom API
interface CreatedRoomResponse extends Record<string, unknown> {
  roomId: number;
}

/**
 * Creates a new room.
 * @param {RoomData} roomData - The data for the new room.
 * @returns {Promise<CreatedRoomResponse>} The created room data, including its new ID.
 */
export const createRoom = async (roomData: RoomData): Promise<CreatedRoomResponse> => {
  const formData = new FormData();
  // Append keys and values from roomData to formData
  (Object.keys(roomData) as Array<keyof RoomData>).forEach(key => {
    if (key === 'hostId') {
      // 讓後端依 JWT 自動指定 HostId
      return;
    }
    formData.append(key, String(roomData[key]));
  });

  const response = await apiClient.post<CreatedRoomResponse>('/Rooms', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  const data = response.data ?? {};
  const record = data as Record<string, unknown>;
  const roomId =
    (typeof record.roomId === 'number' ? record.roomId : undefined) ??
    (typeof record.RoomId === 'number' ? (record.RoomId as number) : undefined);

  if (roomId === undefined) {
    throw new Error('Room ID is missing in the create room response.');
  }

  return {
    ...record,
    roomId,
  } as CreatedRoomResponse;
};

/**
 * Updates an existing room.
 * @param {number} roomId - The ID of the room to update.
 * @param {Partial<RoomData>} roomData - The updated data for the room.
 * @returns {Promise<void>}
 */
export const updateRoom = async (roomId: number, roomData: Partial<RoomData>): Promise<void> => {
  const formData = new FormData();
  // Append keys and values from roomData to formData
  (Object.keys(roomData) as Array<keyof Partial<RoomData>>).forEach(key => {
    formData.append(key, String(roomData[key]));
  });

  await apiClient.put(`/Rooms/${roomId}`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
};

/**
 * Fetches all rooms for a specific landlord.
 * @param {number} hostId - The ID of the landlord.
 * @returns {Promise<any>} A promise that resolves with the list of rooms.
 */
export const getRoomsByHostId = async (_hostId: number) => {
  // 為了相容既有呼叫，忽略傳入參數，改用登入者身份
  const response = await apiClient.get(`/Rooms/host/me`);
  return response.data;
};

export const getMyRooms = async () => {
  const response = await apiClient.get(`/Rooms/host/me`);
  return response.data;
};

/**
 * Deletes a room.
 * @param {number} roomId - The ID of the room to delete.
 * @returns {Promise<void>}
 */
export const deleteRoom = async (roomId: number): Promise<void> => {
  await apiClient.delete(`/Rooms/${roomId}`);
};
