import { apiClient } from './config';
import { Connection, ConnectionFormData } from '../../types/Connection';
import { Authorizer } from '../../types/Authorizer';

/**
 * Service for handling connection-related API operations
 */
export const connectionService = {
  /**
   * Fetches all connections
   */
  getAllConnections: async (): Promise<Connection[]> => {
    try {
      const response = await apiClient.get<Connection[]>('/manage/connections');
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error('Error fetching connections:', error);
      throw error;
    }
  },

  /**
   * Creates a new connection
   */
  createConnection: async (connectionData: ConnectionFormData): Promise<Connection> => {
    try {
      const response = await apiClient.post<Connection>('/manage/connection', connectionData);
      return response.data;
    } catch (error) {
      console.error('Error creating connection:', error);
      throw error;
    }
  },

  /**
   * Updates an existing connection
   */
  updateConnection: async (id: string, connectionData: ConnectionFormData): Promise<Connection> => {
    try {
      const response = await apiClient.put<Connection>(`/manage/connection/${id}`, {
        id,
        ...connectionData
      });
      return response.data;
    } catch (error) {
      console.error(`Error updating connection with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Deletes a connection
   */
  deleteConnection: async (id: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/connection/${id}`);
    } catch (error) {
      console.error(`Error deleting connection with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Fetches a specific connection by ID
   */
  getConnectionById: async (id: string): Promise<Connection> => {
    try {
      const response = await apiClient.get<Connection>(`/manage/connection/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching connection with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Gets authorizers for a connection
   */
  getAuthorizersForConnection: async (connectionId: string): Promise<Authorizer[]> => {
    try {
      const response = await apiClient.get<Authorizer[]>(`/manage/connection/${connectionId}/authorizers`);
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error(`Error fetching authorizers for connection ${connectionId}:`, error);
      throw error;
    }
  },

  /**
   * Adds an authorizer to a connection
   */
  addAuthorizerToConnection: async (connectionId: string, authorizerId: string): Promise<void> => {
    try {
      await apiClient.post(`/manage/connection/${connectionId}/authorizer/${authorizerId}`);
    } catch (error) {
      console.error(`Error adding authorizer ${authorizerId} to connection ${connectionId}:`, error);
      throw error;
    }
  },

  /**
   * Removes an authorizer from a connection
   */
  removeAuthorizerFromConnection: async (connectionId: string, authorizerId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/connection/${connectionId}/authorizer/${authorizerId}`);
    } catch (error) {
      console.error(`Error removing authorizer ${authorizerId} from connection ${connectionId}:`, error);
      throw error;
    }
  }
}; 