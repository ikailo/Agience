import axios from 'axios';

// Create axios instance with default config
export const apiClient = axios.create({
  baseURL: '', // Empty string to use relative URLs with the proxy
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor for auth headers
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Add response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized access
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
); 