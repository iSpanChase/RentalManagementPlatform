import apiClient from './axiosInstance';

export interface City {
  cityId: number;
  cityName: string;
}

export interface District {
  districtId: number;
  districtName: string;
}

/**
 * Fetches a list of all cities.
 * @returns {Promise<City[]>} A list of cities.
 */
export const fetchCities = async (): Promise<City[]> => {
  const response = await apiClient.get<City[]>('/Locations/cities');
  return response.data;
};

/**
 * Fetches a list of districts for a given city.
 * @param {number} cityId The ID of the city.
 * @returns {Promise<District[]>} A list of districts.
 */
export const fetchDistricts = async (cityId: number): Promise<District[]> => {
  if (!cityId) return [];
  const response = await apiClient.get<District[]>(`/Locations/districts/${cityId}`);
  return response.data;
};
