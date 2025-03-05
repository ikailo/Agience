import React, { useState, useEffect } from 'react';
import { agentService } from '../../services/api/agentService';
import { agentTopicService } from '../../services/api/agentTopicService';
import { topicService } from '../../services/api/topicService';
import { Topic, TopicFormData } from '../../types/Topic';
import { Agent } from '../../types/Agent';
import TopicCard from './topics/TopicCard';
import TopicForm from './topics/TopicForm';

interface AgentTopicsTabProps {
  agentId: string;
}

/**
 * AgentTopicsTab component that displays and manages topics for an agent
 * Supports both light and dark modes
 */
function AgentTopicsTab({ agentId }: AgentTopicsTabProps) {
  const [agent, setAgent] = useState<Agent | null>(null);
  const [assignedTopics, setAssignedTopics] = useState<Topic[]>([]);
  const [availableTopics, setAvailableTopics] = useState<Topic[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [showTopicForm, setShowTopicForm] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  /**
   * Fetches the agent details
   */
  const fetchAgentDetails = async () => {
    try {
      const agentData = await agentService.getAgentById(agentId);
      setAgent(agentData);
    } catch (error) {
      console.error('Error fetching agent details:', error);
    }
  };

  /**
   * Fetches all topics assigned to the agent
   */
  const fetchAssignedTopics = async () => {
    try {
      setIsLoading(true);
      const topics = await agentTopicService.getAgentTopics(agentId);
      setAssignedTopics(topics);
    } catch (error) {
      console.error('Error fetching assigned topics:', error);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Fetches all available topics
   */
  const fetchAvailableTopics = async () => {
    try {
      const topics = await topicService.getAllTopics();
      setAvailableTopics(topics);
    } catch (error) {
      console.error('Error fetching available topics:', error);
    }
  };

  // Fetch data on component mount
  useEffect(() => {
    fetchAgentDetails();
    fetchAssignedTopics();
    fetchAvailableTopics();
  }, [agentId]);

  /**
   * Toggles a topic's assignment to the agent
   * @param topicId - The ID of the topic to toggle
   */
  const handleToggleTopic = async (topicId: string) => {
    try {
      setIsLoading(true);
      
      // Check if the topic is already assigned
      const isAssigned = assignedTopics.some(topic => topic.id === topicId);
      
      if (isAssigned) {
        // Remove the topic from the agent
        await agentTopicService.removeTopicFromAgent(agentId, topicId);
      } else {
        // Add the topic to the agent
        await agentTopicService.addTopicToAgent(agentId, topicId);
      }
      
      // Refresh the assigned topics
      fetchAssignedTopics();
    } catch (error) {
      console.error('Error toggling topic:', error);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Filters topics based on search term
   * @param topics - The topics to filter
   * @returns Filtered topics
   */
  const filterTopics = (topics: Topic[]) => {
    if (!searchTerm) return topics;
    
    return topics.filter(topic => 
      topic.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      topic.description.toLowerCase().includes(searchTerm.toLowerCase())
    );
  };

  /**
   * Handles creating a new topic
   * @param topicData - The topic data to save
   */
  const handleSaveTopic = async (topicData: TopicFormData) => {
    try {
      setIsSubmitting(true);
      
      // Create new topic
      await topicService.createTopic(topicData);
      
      // Refresh topics
      fetchAvailableTopics();
      
      // Close form
      handleCancelForm();
    } catch (error) {
      console.error('Error saving topic:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  /**
   * Opens the topic form for creating a new topic
   */
  const handleCreateTopic = () => {
    setShowTopicForm(true);
  };

  /**
   * Closes the topic form
   */
  const handleCancelForm = () => {
    setShowTopicForm(false);
  };

  // Get the IDs of assigned topics for easy lookup
  const assignedTopicIds = assignedTopics.map(topic => topic.id);
  
  // Filter available topics to exclude already assigned ones
  const unassignedTopics = availableTopics.filter(topic => !assignedTopicIds.includes(topic.id));

  // Apply search filter
  const filteredAssignedTopics = filterTopics(assignedTopics);
  const filteredUnassignedTopics = filterTopics(unassignedTopics);

  return (
    <div className="space-y-6">
      {showTopicForm ? (
        <TopicForm
          onSubmit={handleSaveTopic}
          onCancel={handleCancelForm}
          isLoading={isSubmitting}
        />
      ) : (
        <>
          <div className="flex flex-wrap justify-between items-center gap-3">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
              {agent ? `Topics for ${agent.name}` : 'Agent Topics'}
            </h2>
            <button
              onClick={handleCreateTopic}
              className="px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors flex items-center space-x-1 text-sm font-medium shadow-sm"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              <span>New Topic</span>
            </button>
          </div>

          {/* Search box
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg className="h-5 w-5 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <input
              type="text"
              placeholder="Search topics..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-md text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div> */}

          {isLoading ? (
            <div className="flex justify-center py-12">
              <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
            </div>
          ) : (
            <div className="space-y-8">
              {/* Assigned Topics Section */}
              <div>
                <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
                  Assigned Topics ({filteredAssignedTopics.length})
                </h3>
                {filteredAssignedTopics.length === 0 ? (
                  <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                    <p className="text-gray-600 dark:text-gray-400">
                      {searchTerm ? 'No matching assigned topics found.' : 'No topics assigned to this agent yet.'}
                    </p>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {filteredAssignedTopics.map(topic => (
                      <TopicCard
                        key={topic.id}
                        topic={topic}
                        isAssigned={true}
                        onToggle={handleToggleTopic}
                      />
                    ))}
                  </div>
                )}
              </div>

              {/* Available Topics Section */}
              <div>
                <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
                  Available Topics ({filteredUnassignedTopics.length})
                </h3>
                {filteredUnassignedTopics.length === 0 ? (
                  <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                    <p className="text-gray-600 dark:text-gray-400">
                      {searchTerm ? 'No matching available topics found.' : 'All available topics are already assigned.'}
                    </p>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {filteredUnassignedTopics.map(topic => (
                      <TopicCard
                        key={topic.id}
                        topic={topic}
                        isAssigned={false}
                        onToggle={handleToggleTopic}
                      />
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default AgentTopicsTab;