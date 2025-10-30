import apiClient from './axiosInstance';

// Define the shape of a review based on ReviewDto
export interface Review {
  reviewId: number;
  reviewerName: string;
  rating: number;
  comment: string;
  createdAt: string;
}

// Define the shape of the data for creating a review based on CreateReviewDto
export interface CreateReviewPayload {
  bookingId: number; // This might need to be sourced from somewhere
  roomId: number;
  rating: number;
  comment: string;
}

export const fetchReviewsByRoomId = async (roomId: number): Promise<Review[]> => {
  const response = await apiClient.get(`/reviews/room/${roomId}`);
  return response.data;
};

export const createReview = async (payload: CreateReviewPayload): Promise<Review> => {
  const response = await apiClient.post('/reviews', payload);
  return response.data;
};
