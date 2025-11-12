import apiClient from './axiosInstance';

export interface AssistantPlace {
  name: string;
  lat: number;
  lng: number;
}

export interface AssistantChatData {
  place?: AssistantPlace | null;
  rooms?: unknown[];
  radiusKm?: number;
  sortByDistance?: boolean;
}

export interface AssistantChatResponse {
  message: string;
  data: AssistantChatData;
}

export async function assistantChat(text: string): Promise<AssistantChatResponse> {
  const { data } = await apiClient.post<AssistantChatResponse>('/assistant/chat', { text });
  return data;
}

