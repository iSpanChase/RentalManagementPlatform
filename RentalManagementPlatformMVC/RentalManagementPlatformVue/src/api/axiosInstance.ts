import axios from 'axios';

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7230/api';

const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: false,
});

// Add a request interceptor to attach Bearer token
apiClient.interceptors.request.use(
  (config) => {
    // Prefer the same keys used by auth store / http service
    const token =
      localStorage.getItem('rmp.accessToken') ||
      localStorage.getItem('access_token') ||
      localStorage.getItem('jwt_token');

    if (token && token.trim().length > 0) {
      config.headers = config.headers ?? {};
      (config.headers as any)['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default apiClient;
