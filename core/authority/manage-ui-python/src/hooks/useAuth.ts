import { useState, useCallback } from 'react';
import { authService, LoginCredentials } from '../services/endpoints/authService';

export const useAuth = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const login = useCallback(async (credentials: LoginCredentials) => {
    try {
      setIsLoading(true);
      setError(null);
      const response = await authService.login(credentials);
      
      console.log('Login response:', response); // Check what you get here

      if (response.token) {
        localStorage.setItem('token', response.token);
        //console.log('Token set in localStorage:', response.token);
      } else {
        console.error('No token returned in response');
      }

      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      setIsLoading(true);
      await authService.logout();
    } finally {
      setIsLoading(false);
    }
  }, []);

  return {
    login,
    logout,
    isLoading,
    error,
  };
}; 