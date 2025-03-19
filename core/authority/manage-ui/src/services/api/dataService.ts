import { authorizerService } from './authorizerService';
import { connectionService } from './connectionService';
import { Authorizer } from '../../types/Authorizer';
import { Connection } from '../../types/Connection';

/**
 * Service for optimized data operations that combine multiple API calls
 */
export const dataService = {
  /**
   * Gets connections with their authorizer details in a single batch
   */
  getConnectionsWithAuthorizers: async (): Promise<Connection[]> => {
    try {
      // Get all connections
      const connections = await connectionService.getAllConnections();
      
      // Get all authorizers in a single call
      const authorizers = await authorizerService.getAllAuthorizers();
      
      // Create a map for quick lookup
      const authorizerMap = new Map<string, Authorizer>();
      authorizers.forEach(authorizer => {
        authorizerMap.set(authorizer.id, authorizer);
      });
      
      // Attach authorizer objects to connections
      return connections.map(connection => {
        if (connection.authorizer_id && authorizerMap.has(connection.authorizer_id)) {
          return {
            ...connection,
            authorizer: authorizerMap.get(connection.authorizer_id)
          };
        }
        return connection;
      });
    } catch (error) {
      console.error('Error fetching connections with authorizers:', error);
      throw error;
    }
  },

  /**
   * Gets a connection with its authorizer details
   */
  getConnectionWithAuthorizer: async (connectionId: string): Promise<Connection> => {
    try {
      const connection = await connectionService.getConnectionById(connectionId);
      
      if (connection.authorizer_id) {
        try {
          const authorizer = await authorizerService.getAuthorizerById(connection.authorizer_id);
          return {
            ...connection,
            authorizer
          };
        } catch (error) {
          console.error(`Error fetching authorizer for connection ${connectionId}:`, error);
          return connection;
        }
      }
      
      return connection;
    } catch (error) {
      console.error(`Error fetching connection ${connectionId} with authorizer:`, error);
      throw error;
    }
  },

  /**
   * Gets an authorizer with its connections
   */
  getAuthorizerWithConnections: async (authorizerId: string): Promise<{ authorizer: Authorizer, connections: Connection[] }> => {
    try {
      const [authorizer, connections] = await Promise.all([
        authorizerService.getAuthorizerById(authorizerId),
        authorizerService.getConnectionsForAuthorizer(authorizerId)
      ]);
      
      return { authorizer, connections };
    } catch (error) {
      console.error(`Error fetching authorizer ${authorizerId} with connections:`, error);
      throw error;
    }
  }
}; 