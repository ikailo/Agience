import { apiClient } from './config';
import { Agent } from '../../types/Agent';

/**
 * Service for handling topic-agent related API operations
 */
export const topicAgentService = {
  /**
   * Fetches all agents for a specific topic
   * @param topicId - The ID of the topic
   * @param all - Whether to include all agents (including those not owned by the current user)
   * @returns Promise containing an array of Agent objects
   */
  getAgentsForTopic: async (topicId: string, all: boolean = false): Promise<Agent[]> => {
    try {
      const response = await apiClient.get<Agent[]>(`/manage/topic/${topicId}/agents`, {
        params: { all }
      });
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error(`Error fetching agents for topic ${topicId}:`, error);
      throw error;
    }
  },

  /**
   * Adds an agent to a topic
   * @param topicId - The ID of the topic
   * @param agentId - The ID of the agent to add
   * @param all - Whether to include all agents (including those not owned by the current user)
   * @returns Promise indicating success
   */
  addAgentToTopic: async (topicId: string, agentId: string, all: boolean = false): Promise<void> => {
    try {
      await apiClient.post(`/manage/topic/${topicId}/agent/${agentId}`, null, {
        params: { all }
      });
    } catch (error) {
      console.error(`Error adding agent ${agentId} to topic ${topicId}:`, error);
      throw error;
    }
  },

  /**
   * Removes an agent from a topic
   * @param topicId - The ID of the topic
   * @param agentId - The ID of the agent to remove
   * @returns Promise indicating success
   */
  removeAgentFromTopic: async (topicId: string, agentId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/topic/${topicId}/agent/${agentId}`);
    } catch (error) {
      console.error(`Error removing agent ${agentId} from topic ${topicId}:`, error);
      throw error;
    }
  }
}; 