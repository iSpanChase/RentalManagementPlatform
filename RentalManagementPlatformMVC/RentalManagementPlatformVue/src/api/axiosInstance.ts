import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7230/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor
apiClient.interceptors.request.use(
  config => {
    // Retrieve the token from localStorage
    const token = localStorage.getItem('jwt_token');
    // If the token exists, add the Authorization header
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  error => {
    // Do something with request error
    return Promise.reject(error);
  }
);

export default apiClient;
