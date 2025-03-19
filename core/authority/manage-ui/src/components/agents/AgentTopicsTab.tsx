import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Topic, TopicFormData } from '../../types/Topic';
import { Agent } from '../../types/Agent';
import { agentService } from '../../services/api/agentService';
import { agentTopicService } from '../../services/api/agentTopicService';
import { topicService } from '../../services/api/topicService';
import AgentList from './AgentList';
import TopicCard from './topics/TopicCard';
import TopicForm from './topics/TopicForm';

interface AgentTopicsTabProps {
  agentId?: string;
}

/**
 * AgentTopicsTab component that displays and manages topics for an agent
 */
function AgentTopicsTab({ agentId: propAgentId }: AgentTopicsTabProps) {
  const [searchParams, setSearchParams] = useSearchParams();
  const urlAgentId = searchParams.get('id');
  
  // Use the prop agentId if provided, otherwise use the URL parameter
  const contextAgentId = propAgentId || urlAgentId;

  const [agents, setAgents] = useState<Agent[]>([]);
  const [, setSelectedAgent] = useState<Agent | null>(null);
  const [agent, setAgent] = useState<Agent | null>(null);
  const [assignedTopics, setAssignedTopics] = useState<Topic[]>([]);
  const [availableTopics, setAvailableTopics] = useState<Topic[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm] = useState('');
  const [showTopicForm, setShowTopicForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Fetch all agents
  const fetchAgents = useCallback(async () => {
    try {
      const agentsData = await agentService.getAllAgents();
      setAgents(agentsData);
    } catch (error) {
      console.error('Error fetching agents:', error);
    }
  }, []);

  // Fetch agent details
  const fetchAgentDetails = useCallback(async () => {
    if (!contextAgentId) return;

    try {
      const agentData = await agentService.getAgentById(contextAgentId);
      setAgent(agentData);
      setSelectedAgent(agentData);
    } catch (error) {
      console.error('Error fetching agent details:', error);
    }
  }, [contextAgentId]);

  // Fetch assigned topics
  const fetchAssignedTopics = useCallback(async () => {
    if (!contextAgentId) return;

    try {
      const topics = await agentTopicService.getAgentTopics(contextAgentId);
      setAssignedTopics(topics);
    } catch (error) {
      console.error('Error fetching assigned topics:', error);
    }
  }, [contextAgentId]);

  // Fetch available topics
  const fetchAvailableTopics = useCallback(async () => {
    try {
      const topics = await agentTopicService.getAvailableTopics();
      setAvailableTopics(topics);
    } catch (error) {
      console.error('Error fetching available topics:', error);
    }
  }, []);

  // Initial data fetch
  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      await Promise.all([
        fetchAgents(),
        fetchAgentDetails(),
        fetchAssignedTopics(),
        fetchAvailableTopics()
      ]);
      setIsLoading(false);
    };

    fetchData();
  }, [fetchAgents, fetchAgentDetails, fetchAssignedTopics, fetchAvailableTopics]);

  /**
   * Toggles a topic's assignment to the agent
   * @param topicId - The ID of the topic to toggle
   */
  const handleToggleTopic = async (topicId: string) => {
    if (!contextAgentId) return;

    setIsLoading(true);
    try {
      const isAssigned = assignedTopics.some(t => t.id === topicId);

      if (isAssigned) {
        await agentTopicService.removeTopicFromAgent(contextAgentId, topicId);
      } else {
        await agentTopicService.addTopicToAgent(contextAgentId, topicId);
      }

      // Refresh the topic lists
      await fetchAssignedTopics();
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
    
    const term = searchTerm.toLowerCase();
    return topics.filter(topic => 
      topic.name.toLowerCase().includes(term) ||
      topic.description.toLowerCase().includes(term)
    );
  };

  /**
   * Handles creating a new topic
   * @param topicData - The topic data to save
   */
  const handleSaveTopic = async (topicData: TopicFormData) => {
    setIsSubmitting(true);
    try {
      await topicService.createTopic(topicData);
      setShowTopicForm(false);
      await fetchAvailableTopics();
    } catch (error) {
      console.error('Error creating topic:', error);
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

  // Handle agent selection
  const handleSelectAgent = (id: string) => {
    // Update URL with selected agent ID
    setSearchParams({ id });
  };

  // Handle create new agent
  const handleCreateAgent = () => {
    // Navigate to the Details tab for creating a new agent
    const newParams = new URLSearchParams(searchParams);
    newParams.delete('id');
    newParams.set('tab', 'Agents');
    setSearchParams(newParams);
  };

  // Get assigned topic IDs for quick lookup
  const assignedTopicIds = assignedTopics.map(t => t.id);

  // Filter available topics to exclude already assigned ones
  const unassignedTopics = availableTopics.filter(topic => !assignedTopicIds.includes(topic.id));

  // Apply search filter
  const filteredAssignedTopics = filterTopics(assignedTopics);
  const filteredUnassignedTopics = filterTopics(unassignedTopics);

  return (
    <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
      <div className="lg:col-span-1">
        <AgentList
          agents={agents}
          selectedAgentId={contextAgentId || null}
          isLoading={isLoading}
          onSelectAgent={handleSelectAgent}
          onCreateAgent={handleCreateAgent}
        />
      </div>
      
      <div className="lg:col-span-3">
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
                  {agent ? `Topics for ` : 'Agent Topics'}
                  {agent && <span className="dark:text-indigo-500 font-bold text-xl text-indigo-700">{agent.name}</span>}
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

              {!contextAgentId ? (
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center shadow-lg">
                  <p className="text-gray-600 dark:text-gray-300">Please select an agent from the list first.</p>
                </div>
              ) : (
                <>
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
                        <div className="grid grid-cols-1 gap-4">
                          {filteredAssignedTopics.map(topic => (
                            <TopicCard
                              key={topic.id}
                              topic={topic}
                              isAssigned={true}
                              onToggle={handleToggleTopic}
                            />
                          ))}
                          {filteredAssignedTopics.length === 0 && (
                            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                              <p className="text-gray-600 dark:text-gray-400">
                                No topics assigned to this agent yet.
                              </p>
                            </div>
                          )}
                        </div>
                      </div>

                      {/* Available Topics Section */}
                      <div>
                        <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
                          Available Topics ({filteredUnassignedTopics.length})
                        </h3>
                        <div className="grid grid-cols-1 gap-4">
                          {filteredUnassignedTopics.map(topic => (
                            <TopicCard
                              key={topic.id}
                              topic={topic}
                              isAssigned={false}
                              onToggle={handleToggleTopic}
                            />
                          ))}
                          {filteredUnassignedTopics.length === 0 && (
                            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                              <p className="text-gray-600 dark:text-gray-400">
                                No additional topics available.
                              </p>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  )}
                </>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default AgentTopicsTab;