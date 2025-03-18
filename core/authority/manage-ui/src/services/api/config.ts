import { UserManager } from 'oidc-client-ts';
import { AuthConfig } from '../../auth/AuthConfig';
import axios from 'axios';

const userManager = new UserManager(AuthConfig);

// Create axios instance
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_AUTHORITY_PUBLIC_URI || 'https://localhost:5001',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add an async request interceptor to retrieve the token from OIDC client
apiClient.interceptors.request.use(async (config) => {
  try {
    const user = await userManager.getUser();
    if (user && user.access_token) {
      config.headers.Authorization = `Bearer ${user.access_token}`;
    }
  } catch (error) {
    console.error('Error retrieving user from OIDC client', error);
  }
  return config;
});

export default apiClient;
