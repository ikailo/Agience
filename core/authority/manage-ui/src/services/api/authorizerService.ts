import { apiClient } from './config';
import { Authorizer, AuthorizerFormData } from '../../types/Authorizer';
import { Connection } from '../../types/Connection';

/**
 * Service for handling authorizer-related API operations
 */
export const authorizerService = {
  /**
   * Fetches all authorizers
   */
  getAllAuthorizers: async (): Promise<Authorizer[]> => {
    try {
      const response = await apiClient.get<Authorizer[]>('/manage/authorizers');
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error('Error fetching authorizers:', error);
      throw error;
    }
  },

  /**
   * Creates a new authorizer
   */
  createAuthorizer: async (authorizerData: AuthorizerFormData): Promise<Authorizer> => {
    try {
      const response = await apiClient.post<Authorizer>('/manage/authorizer', authorizerData);
      return response.data;
    } catch (error) {
      console.error('Error creating authorizer:', error);
      throw error;
    }
  },

  /**
   * Updates an existing authorizer
   */
  updateAuthorizer: async (id: string, authorizerData: AuthorizerFormData): Promise<Authorizer> => {
    try {
      const response = await apiClient.put<Authorizer>(`/manage/authorizer/${id}`, {
        id,
        ...authorizerData
      });
      return response.data;
    } catch (error) {
      console.error(`Error updating authorizer with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Deletes an authorizer
   */
  deleteAuthorizer: async (id: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/authorizer/${id}`);
    } catch (error) {
      console.error(`Error deleting authorizer with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Fetches a specific authorizer by ID
   */
  getAuthorizerById: async (id: string): Promise<Authorizer> => {
    try {
      const response = await apiClient.get<Authorizer>(`/manage/authorizer/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching authorizer with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Gets connections for an authorizer
   */
  getConnectionsForAuthorizer: async (authorizerId: string): Promise<Connection[]> => {
    try {
      const response = await apiClient.get<Connection[]>(`/manage/authorizer/${authorizerId}/connections`);
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error(`Error fetching connections for authorizer ${authorizerId}:`, error);
      throw error;
    }
  },

  /**
   * Adds a connection to an authorizer
   */
  addConnectionToAuthorizer: async (authorizerId: string, connectionId: string): Promise<void> => {
    try {
      await apiClient.post(`/manage/authorizer/${authorizerId}/connection/${connectionId}`);
    } catch (error) {
      console.error(`Error adding connection ${connectionId} to authorizer ${authorizerId}:`, error);
      throw error;
    }
  },

  /**
   * Removes a connection from an authorizer
   */
  removeConnectionFromAuthorizer: async (authorizerId: string, connectionId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/authorizer/${authorizerId}/connection/${connectionId}`);
    } catch (error) {
      console.error(`Error removing connection ${connectionId} from authorizer ${authorizerId}:`, error);
      throw error;
    }
  }
}; 