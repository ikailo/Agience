import { apiClient } from './config';
import { Topic } from '../../types/Topic';

/**
 * Interface for AgentTopic objects
 */
export interface AgentTopic {
  id: string;
  agentId: string;
  topicId: string;
  topic?: Topic;
  created_date?: string;
}

/**
 * Service for handling agent topic-related API operations
 */
export const agentTopicService = {
  /**
   * Fetches all topics for a specific agent
   * @param agentId - The ID of the agent
   * @returns Promise containing an array of Topic objects
   */
  getAgentTopics: async (agentId: string): Promise<Topic[]> => {
    try {
      const response = await apiClient.get<Topic[]>(`/manage/agent/${agentId}/topics`);
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error(`Error fetching topics for agent ${agentId}:`, error);
      throw error;
    }
  },

  /**
   * Adds a topic to an agent
   * @param agentId - The ID of the agent
   * @param topicId - The ID of the topic to add
   * @returns Promise containing the created AgentTopic object
   */
  addTopicToAgent: async (agentId: string, topicId: string): Promise<AgentTopic> => {
    try {
      const response = await apiClient.post<AgentTopic>(`/manage/agent/${agentId}/topic/${topicId}`);
      return response.data;
    } catch (error) {
      console.error(`Error adding topic ${topicId} to agent ${agentId}:`, error);
      throw error;
    }
  },

  /**
   * Removes a topic from an agent
   * @param agentId - The ID of the agent
   * @param topicId - The ID of the topic to remove
   * @returns Promise indicating success
   */
  removeTopicFromAgent: async (agentId: string, topicId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/agent/${agentId}/topic/${topicId}`);
    } catch (error) {
      console.error(`Error removing topic ${topicId} from agent ${agentId}:`, error);
      throw error;
    }
  },

  /**
   * Fetches all available topics that can be added to an agent
   * @returns Promise containing an array of Topic objects
   */
  getAvailableTopics: async (): Promise<Topic[]> => {
    try {
      const response = await apiClient.get<Topic[]>('/manage/topics');
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error('Error fetching available topics:', error);
      throw error;
    }
  }
}; 