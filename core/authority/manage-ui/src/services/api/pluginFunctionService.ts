import { apiClient } from './config';

export interface Function {
  id: string;
  name: string;
  description: string;
  pluginId: string;
  plugin?: any;
}

export interface FunctionFormData {
  name: string;
  description: string;
}

/**
 * Service for managing plugin functions
 */
const pluginFunctionService = {
  /**
   * Fetches all functions for a plugin
   */
  getFunctionsForPlugin: async (pluginId: string): Promise<Function[]> => {
    try {
      const response = await apiClient.get(`/manage/plugin/${pluginId}/functions`);
      return response.data;
    } catch (error) {
      console.error('Error fetching functions for plugin:', error);
      throw error;
    }
  },

  /**
   * Creates a new function for a plugin
   */
  createFunctionForPlugin: async (pluginId: string, functionData: FunctionFormData): Promise<Function> => {
    try {
      const response = await apiClient.post(`/manage/plugin/${pluginId}/function`, functionData);
      return response.data;
    } catch (error) {
      console.error('Error creating function for plugin:', error);
      throw error;
    }
  },

  /**
   * Adds an existing function to a plugin
   */
  addFunctionToPlugin: async (pluginId: string, functionId: string): Promise<void> => {
    try {
      await apiClient.post(`/manage/plugin/${pluginId}/function/${functionId}`);
    } catch (error) {
      console.error('Error adding function to plugin:', error);
      throw error;
    }
  },

  /**
   * Removes a function from a plugin
   */
  removeFunctionFromPlugin: async (pluginId: string, functionId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/plugin/${pluginId}/function/${functionId}`);
    } catch (error) {
      console.error('Error removing function from plugin:', error);
      throw error;
    }
  },

  /**
   * Updates a function
   */
  updateFunction: async (functionId: string, functionData: FunctionFormData): Promise<Function> => {
    try {
      const response = await apiClient.put(`/manage/function/${functionId}`, functionData);
      return response.data;
    } catch (error) {
      console.error('Error updating function:', error);
      throw error;
    }
  },

  /**
   * Deletes a function
   */
  deleteFunction: async (functionId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/function/${functionId}`);
    } catch (error) {
      console.error('Error deleting function:', error);
      throw error;
    }
  }
};

export default pluginFunctionService; 