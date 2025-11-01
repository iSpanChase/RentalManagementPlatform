import apiClient from '@/api/axiosInstance';
import type { RoomCard } from '@/api/roomSearchApi';

export interface RecommendationRequest {
    guestId?: number | null;
    topN?: number;
    displayM?: number;
}

export const getGuestRecommendations = async (request: RecommendationRequest): Promise<
    RoomCard[]> => {
    const response = await apiClient.post('/ReportForm/Recommendation/ForGuest', request);
    return response.data;
};