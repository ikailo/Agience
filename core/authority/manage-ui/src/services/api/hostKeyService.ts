import { apiClient } from './config';

// Backend API interface (snake_case)
interface ApiKey {
  id: string;
  name: string;
  host_id: string;
  is_active: boolean;
  value?: string;
  is_encrypted?: boolean;
  created_date: string;
}

// Frontend interface (camelCase)
export interface Key {
  id: string;
  name: string;
  hostId: string;
  isActive: boolean;
  value?: string;
  isEncrypted?: boolean;
  created_date: string;
}

export interface KeyFormData {
  name: string;
  isActive: boolean;
}

/**
 * Converts API response (snake_case) to frontend model (camelCase)
 */
const toFrontendKey = (apiKey: ApiKey): Key => {
  return {
    id: apiKey.id,
    name: apiKey.name,
    hostId: apiKey.host_id,
    isActive: !!apiKey.is_active,
    value: apiKey.value,
    isEncrypted: apiKey.is_encrypted,
    created_date: apiKey.created_date
  };
};

/**
 * Converts frontend model (camelCase) to API request format (snake_case)
 */
const toApiRequest = (keyData: Partial<Key>): Partial<ApiKey> => {
  const apiData: Partial<ApiKey> = {
    id: keyData.id,
    name: keyData.name,
    is_active: keyData.isActive
  };
  
  if (keyData.hostId) {
    apiData.host_id = keyData.hostId;
  }
  
  return apiData;
};

/**
 * Service for handling host key-related API operations
 */
export const hostKeyService = {
  /**
   * Fetches all keys for a specific host
   * @param hostId - The ID of the host
   * @returns Promise containing an array of Key objects
   */
  getKeysForHost: async (hostId: string): Promise<Key[]> => {
    try {
      console.log(`Fetching keys for host ID: ${hostId}`);
      
      // The controller endpoint might be different from what we're using
      // Try with the correct endpoint based on the controller code
      const endpoint = `/manage/host/${hostId}/keys`;
      console.log(`Using API endpoint: ${endpoint}`);
      
      const response = await apiClient.get<ApiKey[]>(endpoint);
      // console.log('API Response status:', response.status);
      // console.log('API Response data type:', typeof response.data);
      
      const keys = Array.isArray(response.data) ? response.data : [];
      console.log('Raw keys data:', keys);
      
      // Convert API response to frontend model
      const frontendKeys = keys.map(toFrontendKey);
      console.log('Converted frontend keys:', frontendKeys);
      
      return frontendKeys;
    } catch (error) {
      console.error(`Error fetching keys for host ${hostId}:`, error);
      throw error;
    }
  },

  /**
   * Generates a new key for a host
   * @param hostId - The ID of the host
   * @param keyData - The key data to create
   * @returns Promise containing the created Key object with the secret value
   */
  generateKeyForHost: async (hostId: string, keyData: KeyFormData): Promise<Key> => {
    try {
      // Convert to API format (snake_case)
      const requestData = {
        name: keyData.name,
        is_active: keyData.isActive
      };
      
      console.log('Sending key generation request:', requestData);
      const response = await apiClient.post<ApiKey>(`/manage/host/${hostId}/key/generate`, requestData);
      console.log('Key generation response:', response.data);
      
      // Convert API response to frontend model
      return toFrontendKey(response.data);
    } catch (error) {
      console.error(`Error generating key for host ${hostId}:`, error);
      throw error;
    }
  },

  /**
   * Updates an existing key
   * @param hostId - The ID of the host
   * @param keyId - The ID of the key to update
   * @param keyData - The updated key data
   * @returns Promise indicating success
   */
  updateKey: async (hostId: string, keyId: string, keyData: Partial<Key>): Promise<void> => {
    try {
      // Convert to API format (snake_case)
      const requestData = toApiRequest({
        id: keyId,
        hostId: hostId,
        ...keyData
      });
      
      console.log('Sending key update request:', requestData);
      await apiClient.put<void>(`/manage/host/${hostId}/key/${keyId}`, requestData);
    } catch (error) {
      console.error(`Error updating key ${keyId} for host ${hostId}:`, error);
      throw error;
    }
  },

  /**
   * Deletes a key
   * @param hostId - The ID of the host
   * @param keyId - The ID of the key to delete
   * @returns Promise indicating success
   */
  deleteKey: async (hostId: string, keyId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/host/${hostId}/key/${keyId}`);
    } catch (error) {
      console.error(`Error deleting key ${keyId} for host ${hostId}:`, error);
      throw error;
    }
  },

  /**
   * Fetches a specific key by ID
   * @param hostId - The ID of the host
   * @param keyId - The ID of the key to fetch
   * @returns Promise containing the Key object
   */
  getKeyById: async (hostId: string, keyId: string): Promise<Key> => {
    try {
      const response = await apiClient.get<ApiKey>(`/manage/host/${hostId}/key/${keyId}`);
      // Convert API response to frontend model
      return toFrontendKey(response.data);
    } catch (error) {
      console.error(`Error fetching key ${keyId} for host ${hostId}:`, error);
      throw error;
    }
  }
}; 