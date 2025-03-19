import { apiClient } from './config';
import { Topic } from '../../types/Topic';
import { Agent } from '../../types/Agent';

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
   * Get all agents assigned to a topic
   * @param topicId - The ID of the topic
   * @returns Promise containing an array of Agent objects
   */
  getTopicAgents: async (topicId: string): Promise<Agent[]> => {
    try {
      const response = await apiClient.get<Agent[]>(`/manage/topic/${topicId}/agents`);
      return response.data;
    } catch (error) {
      console.error('Error fetching topic agents:', error);
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
      console.error('Error adding topic to agent:', error);
      throw error;
    }
  },

  /**
   * Removes a topic from an agent
   * @param agentId - The ID of the agent
   * @param topicId - The ID of the topic to remove
   * @returns Promise containing the deleted AgentTopic object
   */
  removeTopicFromAgent: async (agentId: string, topicId: string): Promise<AgentTopic> => {
    try {
      const response = await apiClient.delete<AgentTopic>(`/manage/agent/${agentId}/topic/${topicId}`);
      return response.data;
    } catch (error) {
      console.error('Error removing topic from agent:', error);
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
  },

  /**
   * Creates a new topic for an agent
   * @param agentId - The ID of the agent
   * @param topicData - The topic data to create
   * @returns Promise containing the created Topic object
   */
  createTopicForAgent: async (agentId: string, topicData: { name: string; description: string }): Promise<Topic> => {
    try {
      const response = await apiClient.post<Topic>(`/manage/agent/${agentId}/topic`, topicData);
      return response.data;
    } catch (error) {
      console.error('Error creating topic for agent:', error);
      throw error;
    }
  }
}; 