import { apiClient } from './config';

/**
 * Interface for Function objects
 */
export interface Function {
  id: string;
  name: string;
  description: string;
  pluginId: string;
  plugin?: any;
}

/**
 * Service for handling function-related API operations
 */
export const functionService = {
  /**
   * Fetches all functions from the API
   * @returns Promise containing an array of Function objects
   */
  getAllFunctions: async (): Promise<Function[]> => {
    try {
      const response = await apiClient.get<Function[]>('/manage/functions');
      
      // Check if response.data is an array
      const functions = Array.isArray(response.data) ? response.data : [];
      return functions;
    } catch (error) {
      console.error('Error fetching functions:', error);
      throw error;
    }
  },

  /**
   * Fetches a specific function by ID
   * @param id - The ID of the function to fetch
   * @returns Promise containing the Function object
   */
  getFunctionById: async (id: string): Promise<Function> => {
    try {
      const response = await apiClient.get<Function>(`/manage/function/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching function with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Fetches all executive functions (functions that can be used as executive functions)
   * @returns Promise containing an array of Function objects
   */
  getExecutiveFunctions: async (): Promise<Function[]> => {
    try {
      const response = await apiClient.get<Function[]>('/manage/functions/executive');
      
      // Check if response.data is an array
      const functions = Array.isArray(response.data) ? response.data : [];
      return functions;
    } catch (error) {
      console.error('Error fetching executive functions:', error);
      // If the specific endpoint fails, fall back to getting all functions
      try {
        return await functionService.getAllFunctions();
      } catch (fallbackError) {
        console.error('Error in fallback fetch of all functions:', fallbackError);
        return [];
      }
    }
  }
}; 