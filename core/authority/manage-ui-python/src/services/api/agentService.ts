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
      // if (agentData.image) {
      //   const formData = new FormData();
        
      //   // Convert camelCase to snake_case for API
      //   const apiData = {
      //     name: agentData.name,
      //     description: agentData.description,
      //     persona: agentData.persona,
      //     host_id: agentData.hostId,  // Convert hostId to host_id
      //     executive_function_id: agentData.executiveFunctionId,  // Convert executiveFunctionId to executive_function_id
      //     is_enabled: agentData.is_enabled
      //   };
        
      //   // Add all agent data to form
      //   Object.entries(apiData).forEach(([key, value]) => {
      //     if (key === 'image') {
      //       // Check if value is a File by type assertion
      //       const fileValue = value as unknown;
      //       if (fileValue instanceof File) {
      //         formData.append('image', fileValue as File);
      //       }
      //     } else if (value !== undefined && value !== null) {
      //       formData.append(key, String(value));
      //     }
      //   });
        
      //   const response = await apiClient.post<Agent>('/manage/agent', formData, {
      //     headers: {
      //       'Content-Type': 'multipart/form-data',
      //     },
      //   });
      //   return response.data;
      // } else {
        // No image, just send JSON with snake_case keys
        const apiData = {
          name: agentData.name,
          description: agentData.description,
          persona: agentData.persona,
          host_id: agentData.hostId,
          executive_function_id: agentData.executiveFunctionId,
          is_enabled: agentData.is_enabled
        };
        const response = await apiClient.post<Agent>('/manage/agent', apiData);
        return response.data;
      // }
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
      console.log('Original agent data for update:', agentData);
      console.log('is_enabled type:', typeof agentData.is_enabled);
      console.log('is_enabled value:', agentData.is_enabled);
      
      // Convert camelCase to snake_case for API
      const apiData = {
        id: id,
        name: agentData.name,
        description: agentData.description,
        persona: agentData.persona || null,
        host_id: agentData.hostId || null,  // Convert from camelCase to snake_case
        executive_function_id: agentData.executiveFunctionId || null,  // Convert from camelCase to snake_case
        is_enabled: agentData.is_enabled
      };
      
      console.log('Sending update data:', apiData);
      
      const response = await apiClient.put<Agent>(`/manage/agent/${id}`, apiData);
      console.log('Update response:', response.data);
      return response.data;
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