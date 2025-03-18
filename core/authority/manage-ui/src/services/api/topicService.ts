import { apiClient } from './config';
import { Topic, TopicFormData } from '../../types/Topic';

/**
 * Service for handling topic-related API operations
 */
export const topicService = {
  /**
   * Fetches all topics from the API
   * @returns Promise containing an array of Topic objects
   */
  getAllTopics: async (): Promise<Topic[]> => {
    try {
      const response = await apiClient.get<Topic[]>('/manage/topics');
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error('Error fetching topics:', error);
      throw error;
    }
  },

  /**
   * Creates a new topic
   * @param topicData - The topic data to create
   * @returns Promise containing the created Topic object
   */
  createTopic: async (topicData: TopicFormData): Promise<Topic> => {
    try {
      const requestData = {
        name: topicData.name,
        description: topicData.description
      };
      
      const response = await apiClient.post<Topic>('/manage/topic', requestData);
      return response.data;
    } catch (error) {
      console.error('Error creating topic:', error);
      throw error;
    }
  },

  /**
   * Updates an existing topic
   * @param id - The ID of the topic to update
   * @param topicData - The updated topic data
   * @returns Promise containing the updated Topic object
   */
  updateTopic: async (id: string, topicData: TopicFormData): Promise<Topic> => {
    try {
      const requestData = {
        id: id,
        name: topicData.name,
        description: topicData.description
      };
      
      const response = await apiClient.put<Topic>(`/manage/topic/${id}`, requestData);
      return response.data;
    } catch (error) {
      console.error(`Error updating topic with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Deletes a topic
   * @param id - The ID of the topic to delete
   * @returns Promise indicating success
   */
  deleteTopic: async (id: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/topic/${id}`);
    } catch (error) {
      console.error(`Error deleting topic with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Fetches a specific topic by ID
   * @param id - The ID of the topic to fetch
   * @returns Promise containing the Topic object
   */
  getTopicById: async (id: string): Promise<Topic> => {
    try {
      const response = await apiClient.get<Topic>(`/manage/topic/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching topic with ID ${id}:`, error);
      throw error;
    }
  }
}; 