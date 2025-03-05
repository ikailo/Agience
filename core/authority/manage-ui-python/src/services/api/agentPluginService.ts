import { apiClient } from './config';
import { Plugin } from '../../types/Plugin';

/**
 * Interface for AgentPlugin objects
 */
export interface AgentPlugin {
  id: string;
  agentId: string;
  pluginId: string;
  plugin?: Plugin;
  created_date?: string;
}

/**
 * Service for handling agent plugin-related API operations
 */
export const agentPluginService = {
  /**
   * Fetches all plugins for a specific agent
   * @param agentId - The ID of the agent
   * @returns Promise containing an array of Plugin objects
   */
  getAgentPlugins: async (agentId: string): Promise<Plugin[]> => {
    try {
      const response = await apiClient.get<Plugin[]>(`/manage/agent/${agentId}/plugins`);
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error(`Error fetching plugins for agent ${agentId}:`, error);
      throw error;
    }
  },

  /**
   * Adds a plugin to an agent
   * @param agentId - The ID of the agent
   * @param pluginId - The ID of the plugin to add
   * @returns Promise containing the created AgentPlugin object
   */
  addPluginToAgent: async (agentId: string, pluginId: string): Promise<AgentPlugin> => {
    try {
      const response = await apiClient.post<AgentPlugin>(`/manage/agent/${agentId}/plugin/${pluginId}`);
      return response.data;
    } catch (error) {
      console.error(`Error adding plugin ${pluginId} to agent ${agentId}:`, error);
      throw error;
    }
  },

  /**
   * Removes a plugin from an agent
   * @param agentId - The ID of the agent
   * @param pluginId - The ID of the plugin to remove
   * @returns Promise indicating success
   */
  removePluginFromAgent: async (agentId: string, pluginId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/agent/${agentId}/plugin/${pluginId}`);
    } catch (error) {
      console.error(`Error removing plugin ${pluginId} from agent ${agentId}:`, error);
      throw error;
    }
  },

  /**
   * Fetches all available plugins that can be added to an agent
   * @returns Promise containing an array of Plugin objects
   */
  getAvailablePlugins: async (): Promise<Plugin[]> => {
    try {
      const response = await apiClient.get<Plugin[]>('/manage/plugins');
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error('Error fetching available plugins:', error);
      throw error;
    }
  }
}; 