import { apiClient } from './config';
import { Host, HostFormData } from '../../types/Host';

/**
 * Service for handling host-related API operations
 */
export const hostService = {
  /**
   * Fetches all hosts from the API
   * @returns Promise containing an array of Host objects
   */
  getAllHosts: async (): Promise<Host[]> => {
    try {
      const response = await apiClient.get<Host[]>('/manage/hosts');
      
      // Check if response.data is an array
      const hosts = Array.isArray(response.data) ? response.data : [];
      return hosts;
    } catch (error) {
      console.error('Error fetching hosts:', error);
      throw error;
    }
  },

  /**
   * Creates a new host
   * @param hostData - The host data to create
   * @returns Promise containing the created Host object
   */
  createHost: async (hostData: HostFormData): Promise<Host> => {
    try {
      const response = await apiClient.post<Host>('/manage/host', hostData);
      return response.data;
    } catch (error) {
      console.error('Error creating host:', error);
      throw error;
    }
  },

  /**
   * Updates an existing host
   * @param id - The ID of the host to update
   * @param hostData - The updated host data
   * @returns Promise containing the updated Host object
   */
  updateHost: async (id: string, hostData: HostFormData): Promise<Host> => {
    try {
      const response = await apiClient.put<Host>(`/manage/host/${id}`, {
        ...hostData,
        id // Include the ID in the request body as required by the API
      });
      return response.data;
    } catch (error) {
      console.error(`Error updating host with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Deletes a host
   * @param id - The ID of the host to delete
   * @returns Promise indicating success
   */
  deleteHost: async (id: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/host/${id}`);
    } catch (error) {
      console.error(`Error deleting host with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Fetches a specific host by ID
   * @param id - The ID of the host to fetch
   * @returns Promise containing the Host object
   */
  getHostById: async (id: string): Promise<Host> => {
    try {
      const response = await apiClient.get<Host>(`/manage/host/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching host with ID ${id}:`, error);
      throw error;
    }
  }
}; 