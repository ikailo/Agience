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
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';

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
  const [selectedAgent, setSelectedAgent] = useState<Agent | null>(null);
  const [agent, setAgent] = useState<Agent | null>(null);
  const [assignedTopics, setAssignedTopics] = useState<Topic[]>([]);
  const [availableTopics, setAvailableTopics] = useState<Topic[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showTopicForm, setShowTopicForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [notification, setNotification] = useState<{ isOpen: boolean; title: string; message: string; type: 'success' | 'error' | 'info' | 'warning' }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'info'
  });
  const [confirmation, setConfirmation] = useState<{ isOpen: boolean; topicId: string; topicName: string }>({
    isOpen: false,
    topicId: '',
    topicName: ''
  });

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
      if (!contextAgentId) {
        // If no agent is selected, just fetch the list of agents
        setIsLoading(true);
        await fetchAgents();
        setIsLoading(false);
        return;
      }
      
      setIsLoading(true);
      try {
        // Fetch data in parallel to reduce loading time
        const [agentsData, agentData, assignedTopicsData, availableTopicsData] = await Promise.all([
          agentService.getAllAgents(),
          agentService.getAgentById(contextAgentId),
          agentTopicService.getAgentTopics(contextAgentId),
          agentTopicService.getAvailableTopics()
        ]);
        
        setAgents(agentsData);
        setAgent(agentData);
        setSelectedAgent(agentData);
        setAssignedTopics(assignedTopicsData);
        setAvailableTopics(availableTopicsData);
      } catch (error) {
        console.error('Error fetching data:', error);
        showNotification('Error', 'Failed to load data', 'error');
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [contextAgentId]);

  /**
   * Shows a notification modal
   */
  const showNotification = (title: string, message: string, type: 'success' | 'error' | 'info' | 'warning') => {
    setNotification({ isOpen: true, title, message, type });
  };

  /**
   * Closes the notification modal
   */
  const closeNotification = () => {
    setNotification(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Shows the confirmation modal for topic removal
   */
  const showRemoveConfirmation = (topicId: string, topicName: string) => {
    setConfirmation({ isOpen: true, topicId, topicName });
  };

  /**
   * Closes the confirmation modal
   */
  const closeConfirmation = () => {
    setConfirmation(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Handles the actual removal of a topic after confirmation
   */
  const handleRemoveTopic = async () => {
    if (!contextAgentId || !confirmation.topicId) return;

    setIsLoading(true);
    try {
      await agentTopicService.removeTopicFromAgent(contextAgentId, confirmation.topicId);
      await fetchAssignedTopics();
      showNotification(
        'Topic Removed',
        `Successfully removed topic "${confirmation.topicName}" from the agent.`,
        'success'
      );
    } catch (error) {
      console.error('Error removing topic:', error);
      showNotification(
        'Error',
        `Failed to remove topic "${confirmation.topicName}". Please try again.`,
        'error'
      );
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  /**
   * Handles adding a topic to the agent
   */
  const handleAddTopic = async (topicId: string) => {
    if (!contextAgentId) return;

    setIsLoading(true);
    try {
      await agentTopicService.addTopicToAgent(contextAgentId, topicId);
      await fetchAssignedTopics();
      const topic = availableTopics.find(t => t.id === topicId);
      showNotification(
        'Topic Added',
        `Successfully added topic "${topic?.name}" to the agent.`,
        'success'
      );
    } catch (error) {
      console.error('Error adding topic:', error);
      const topic = availableTopics.find(t => t.id === topicId);
      showNotification(
        'Error',
        `Failed to add topic "${topic?.name}". Please try again.`,
        'error'
      );
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Toggles a topic's assignment to the agent
   * @param topicId - The ID of the topic to toggle
   */
  const handleToggleTopic = (topicId: string) => {
    const isAssigned = assignedTopics.some(t => t.id === topicId);
    
    if (isAssigned) {
      const topic = assignedTopics.find(t => t.id === topicId);
      if (topic) {
        showRemoveConfirmation(topicId, topic.name);
      }
    } else {
      handleAddTopic(topicId);
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
      let newTopic;
      
      if (contextAgentId) {
        // If an agent is selected, create the topic directly for the agent
        newTopic = await agentTopicService.createTopicForAgent(contextAgentId, topicData);
        showNotification(
          'Success',
          `Topic "${topicData.name}" created and assigned to agent successfully`,
          'success'
        );
      } else {
        // Otherwise, just create a regular topic
        newTopic = await topicService.createTopic(topicData);
        showNotification(
          'Success',
          `Topic "${topicData.name}" created successfully`,
          'success'
        );
      }
      
      setShowTopicForm(false);
      await fetchAvailableTopics();
      
      // If an agent is selected, also refresh the assigned topics
      if (contextAgentId) {
        await fetchAssignedTopics();
      }
    } catch (error) {
      console.error('Error creating topic:', error);
      showNotification(
        'Error',
        `Failed to create topic "${topicData.name}". Please try again.`,
        'error'
      );
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

      {/* Notification Modal */}
      <NotificationModal
        isOpen={notification.isOpen}
        onClose={closeNotification}
        title={notification.title}
        message={notification.message}
        type={notification.type}
      />

      {/* Confirmation Modal */}
      <ConfirmationModal
        isOpen={confirmation.isOpen}
        onClose={closeConfirmation}
        onConfirm={handleRemoveTopic}
        title="Remove Topic"
        message={`Are you sure you want to remove the topic "${confirmation.topicName}" from this agent?`}
        confirmText="Remove"
        cancelText="Cancel"
        type="danger"
        isLoading={isLoading}
      />
    </div>
  );
}

export default AgentTopicsTab;