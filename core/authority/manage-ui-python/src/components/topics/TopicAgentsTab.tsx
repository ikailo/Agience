import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Agent } from '../../types/Agent';
import { Topic } from '../../types/Topic';
import { agentService } from '../../services/api/agentService';
import { topicService } from '../../services/api/topicService';
import { agentTopicService } from '../../services/api/agentTopicService';
import Card from '../common/Card';

interface TopicAgentsTabProps {
  topicId?: string;
}

/**
 * TopicAgentsTab component that displays and manages agents for a topic
 */
const TopicAgentsTab: React.FC<TopicAgentsTabProps> = ({ topicId: propTopicId }) => {
  const [searchParams, setSearchParams] = useSearchParams();
  const urlTopicId = searchParams.get('id');
  
  // Use the prop topicId if provided, otherwise use the URL parameter
  const contextTopicId = propTopicId || urlTopicId;

  const [topic, setTopic] = useState<Topic | null>(null);
  const [assignedAgents, setAssignedAgents] = useState<Agent[]>([]);
  const [availableAgents, setAvailableAgents] = useState<Agent[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  // Fetch topic details
  const fetchTopicDetails = useCallback(async () => {
    if (!contextTopicId) return;

    try {
      const topicData = await topicService.getTopicById(contextTopicId);
      setTopic(topicData);
    } catch (error) {
      console.error('Error fetching topic details:', error);
    }
  }, [contextTopicId]);

  // Fetch assigned agents
  const fetchAssignedAgents = useCallback(async () => {
    if (!contextTopicId) return;

    try {
      const agents = await agentTopicService.getTopicAgents(contextTopicId);
      setAssignedAgents(agents);
    } catch (error) {
      console.error('Error fetching assigned agents:', error);
    }
  }, [contextTopicId]);

  // Fetch available agents
  const fetchAvailableAgents = useCallback(async () => {
    try {
      const agents = await agentService.getAllAgents();
      setAvailableAgents(agents);
    } catch (error) {
      console.error('Error fetching available agents:', error);
    }
  }, []);

  // Initial data fetch
  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      await Promise.all([
        fetchTopicDetails(),
        fetchAssignedAgents(),
        fetchAvailableAgents()
      ]);
      setIsLoading(false);
    };

    fetchData();
  }, [fetchTopicDetails, fetchAssignedAgents, fetchAvailableAgents]);

  // Handle agent toggle
  const handleToggleAgent = async (agentId: string) => {
    if (!contextTopicId) return;

    setIsLoading(true);
    try {
      const isAssigned = assignedAgents.some(a => a.id === agentId);

      if (isAssigned) {
        await agentTopicService.removeTopicFromAgent(agentId, contextTopicId);
      } else {
        await agentTopicService.addTopicToAgent(agentId, contextTopicId);
      }

      // Refresh the agent lists
      await fetchAssignedAgents();
    } catch (error) {
      console.error('Error toggling agent:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // Filter agents based on search term
  const filterAgents = (agents: Agent[]) => {
    if (!searchTerm) return agents;
    
    const term = searchTerm.toLowerCase();
    return agents.filter(agent => 
      agent.name.toLowerCase().includes(term) ||
      agent.description?.toLowerCase().includes(term)
    );
  };

  // Get assigned agent IDs for quick lookup
  const assignedAgentIds = assignedAgents.map(a => a.id);

  // Filter available agents to exclude already assigned ones
  const unassignedAgents = availableAgents.filter(agent => !assignedAgentIds.includes(agent.id));

  // Apply search filter
  const filteredAssignedAgents = filterAgents(assignedAgents);
  const filteredUnassignedAgents = filterAgents(unassignedAgents);

  // Render agent card
  const renderAgentCard = (agent: Agent, isAssigned: boolean) => (
    <div
      key={agent.id}
      className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 hover:shadow-md transition-shadow"
    >
      <div className="flex items-start justify-between">
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white truncate">
            {agent.name}
          </h3>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400 line-clamp-2">
            {agent.description || 'No description'}
          </p>
        </div>
        <button
          onClick={() => handleToggleAgent(agent.id)}
          className={`ml-4 px-3 py-1.5 text-sm font-medium rounded-md shadow-sm ${
            isAssigned
              ? 'bg-red-600 text-white hover:bg-red-700'
              : 'bg-indigo-600 text-white hover:bg-indigo-700'
          } transition-colors`}
          disabled={isLoading}
        >
          {isAssigned ? 'Remove' : 'Add'}
        </button>
      </div>
    </div>
  );

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap justify-between items-center gap-3">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          {topic ? `Agents for ` : 'Topic Agents'}
          {topic && <span className="dark:text-indigo-500 font-bold text-xl text-indigo-700">{topic.name}</span>}
        </h2>
      </div>

      {!contextTopicId ? (
        <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center shadow-lg">
          <p className="text-gray-600 dark:text-gray-300">Please select a topic from the Details tab first.</p>
        </div>
      ) : (
        <>
          {isLoading ? (
            <div className="flex justify-center py-12">
              <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
            </div>
          ) : (
            <div className="space-y-8">
              {/* Assigned Agents Section */}
              <Card title={`Assigned Agents (${filteredAssignedAgents.length})`}>
                <div className="grid grid-cols-1 gap-4">
                  {filteredAssignedAgents.map(agent => renderAgentCard(agent, true))}
                  {filteredAssignedAgents.length === 0 && (
                    <div className="text-center py-8 px-4">
                      <p className="text-gray-600 dark:text-gray-400">
                        No agents assigned to this topic yet.
                      </p>
                    </div>
                  )}
                </div>
              </Card>

              {/* Available Agents Section */}
              <Card title={`Available Agents (${filteredUnassignedAgents.length})`}>
                <div className="grid grid-cols-1 gap-4">
                  {filteredUnassignedAgents.map(agent => renderAgentCard(agent, false))}
                  {filteredUnassignedAgents.length === 0 && (
                    <div className="text-center py-8 px-4">
                      <p className="text-gray-600 dark:text-gray-400">
                        No additional agents available.
                      </p>
                    </div>
                  )}
                </div>
              </Card>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default TopicAgentsTab; 