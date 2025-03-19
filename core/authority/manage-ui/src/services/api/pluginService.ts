import { apiClient } from './config';
import { Plugin, PluginFormData } from '../../types/Plugin';

/**
 * Service for handling plugin-related API operations
 */
export const pluginService = {
  /**
   * Fetches all plugins from the API
   * @returns Promise containing an array of Plugin objects
   */
  getAllPlugins: async (): Promise<Plugin[]> => {
    try {
      const response = await apiClient.get<Plugin[]>('/manage/plugins');
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error('Error fetching plugins:', error);
      throw error;
    }
  },

  /**
   * Creates a new plugin
   * @param pluginData - The plugin data to create
   * @returns Promise containing the created Plugin object
   */
  createPlugin: async (pluginData: PluginFormData): Promise<Plugin> => {
    try {
      // Convert the provider to the format expected by the API
      const requestData = {
        name: pluginData.name,
        description: pluginData.description,
        provider: pluginData.provider
      };
      
      const response = await apiClient.post<Plugin>('/manage/plugin', requestData);
      return response.data;
    } catch (error) {
      console.error('Error creating plugin:', error);
      throw error;
    }
  },

  /**
   * Updates an existing plugin
   * @param id - The ID of the plugin to update
   * @param pluginData - The updated plugin data
   * @returns Promise containing the updated Plugin object
   */
  updatePlugin: async (id: string, pluginData: PluginFormData): Promise<Plugin> => {
    try {
      const requestData = {
        id: id,
        name: pluginData.name,
        description: pluginData.description,
        provider: pluginData.provider
      };
      
      const response = await apiClient.put<Plugin>(`/manage/plugin/${id}`, requestData);
      return response.data;
    } catch (error) {
      console.error(`Error updating plugin with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Deletes a plugin
   * @param id - The ID of the plugin to delete
   * @returns Promise indicating success
   */
  deletePlugin: async (id: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/plugin/${id}`);
    } catch (error) {
      console.error(`Error deleting plugin with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Fetches a specific plugin by ID
   * @param id - The ID of the plugin to fetch
   * @returns Promise containing the Plugin object
   */
  getPluginById: async (id: string): Promise<Plugin> => {
    try {
      const response = await apiClient.get<Plugin>(`/manage/plugin/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching plugin with ID ${id}:`, error);
      throw error;
    }
  }
}; 