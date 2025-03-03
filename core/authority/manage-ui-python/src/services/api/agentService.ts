import { apiClient } from './config';
import { Agent, AgentFormData } from '../../types/Agent';

/**
 * Service for handling agent-related API operations
 */
export const agentService = {
  /**
   * Creates a new agent
   * @param agentData - The agent data to create
   * @returns Promise containing the created Agent object
   */
  createAgent: async (agentData: AgentFormData): Promise<Agent> => {
    try {
      // Create form data if there's an image to upload
      if (agentData.image) {
        const formData = new FormData();
        
        // Add all agent data to form
        Object.entries(agentData).forEach(([key, value]) => {
          if (key === 'image' && value instanceof File) {
            formData.append('image', value);
          } else if (value !== undefined && value !== null) {
            formData.append(key, String(value));
          }
        });
        
        const response = await apiClient.post<Agent>('/manage/agent', formData, {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        });
        return response.data;
      } else {
        // No image, just send JSON
        const response = await apiClient.post<Agent>('/manage/agent', agentData);
        return response.data;
      }
    } catch (error) {
      console.error('Error creating agent:', error);
      throw error;
    }
  },

  /**
   * Updates an existing agent
   * @param id - The ID of the agent to update
   * @param agentData - The updated agent data
   * @returns Promise containing the updated Agent object
   */
  updateAgent: async (id: string, agentData: AgentFormData): Promise<Agent> => {
    try {
      // Create form data if there's an image to upload
      if (agentData.image) {
        const formData = new FormData();
        
        // Add all agent data to form
        Object.entries(agentData).forEach(([key, value]) => {
          if (key === 'image' && value instanceof File) {
            formData.append('image', value);
          } else if (value !== undefined && value !== null) {
            formData.append(key, String(value));
          }
        });
        
        // Add ID to form data
        formData.append('id', id);
        
        const response = await apiClient.put<Agent>(`/manage/agent/${id}`, formData, {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        });
        return response.data;
      } else {
        // No image, just send JSON
        const response = await apiClient.put<Agent>(`/manage/agent/${id}`, {
          ...agentData,
          id // Include the ID in the request body as required by the API
        });
        return response.data;
      }
    } catch (error) {
      console.error(`Error updating agent with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Deletes an agent
   * @param id - The ID of the agent to delete
   * @returns Promise indicating success
   */
  deleteAgent: async (id: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/agent/${id}`);
    } catch (error) {
      console.error(`Error deleting agent with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Fetches all agents from the API
   * @returns Promise containing an array of Agent objects
   */
  getAllAgents: async (): Promise<Agent[]> => {
    try {
      const response = await apiClient.get<Agent[]>('/manage/agents');
      
      // Check if response.data is an array
      const agents = Array.isArray(response.data) ? response.data : [];
      
      // Add default image URL for UI display if not provided
      return agents.map(agent => ({
        ...agent,
        imageUrl: agent.imageUrl || '/astra-avatar.png'
      }));
    } catch (error) {
      console.error('Error fetching agents:', error);
      throw error;
    }
  },

  /**
   * Fetches a specific agent by ID
   * @param id - The ID of the agent to fetch
   * @returns Promise containing the Agent object
   */
  getAgentById: async (id: string): Promise<Agent> => {
    try {
      const response = await apiClient.get<Agent>(`/manage/agent/${id}`);
      
      // Add default image URL for UI display if not provided
      return {
        ...response.data,
        imageUrl: response.data.imageUrl || '/astra-avatar.png'
      };
    } catch (error) {
      console.error(`Error fetching agent with ID ${id}:`, error);
      throw error;
    }
  }
}; 