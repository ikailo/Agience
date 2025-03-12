import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Agent } from '../../types/Agent';
import { Topic } from '../../types/Topic';
import { topicAgentService } from '../../services/api/topicAgentService';
import { topicService } from '../../services/api/topicService';

/**
 * TopicAgents component that displays agents connected to a topic in a table
 * Supports both light and dark modes and is mobile responsive
 */
function TopicAgents() {
  console.log('TopicAgents component rendering');
  const [searchParams] = useSearchParams();
  const topicId = searchParams.get('id');
  console.log('Topic ID from URL:', topicId);
  
  const [agents, setAgents] = useState<Agent[]>([]);
  const [topic, setTopic] = useState<Topic | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetches the topic details
   */
  const fetchTopicDetails = useCallback(async () => {
    if (!topicId) {
      console.log('No topic ID available, skipping topic details fetch');
      return;
    }
    
    try {
      console.log('Fetching topic details for ID:', topicId);
      const topicData = await topicService.getTopicById(topicId);
      console.log('Topic data received:', topicData);
      setTopic(topicData);
    } catch (err) {
      console.error('Error fetching topic details:', err);
      setError('Failed to load topic details');
    }
  }, [topicId]);

  /**
   * Fetches agents connected to the topic
   */
  const fetchAgents = useCallback(async () => {
    if (!topicId) {
      console.log('No topic ID available, skipping agents fetch');
      return;
    }
    
    try {
      setIsLoading(true);
      console.log('Fetching agents for topic ID:', topicId);
      const agentsData = await topicAgentService.getAgentsForTopic(topicId);
      console.log('Agents data received:', agentsData);
      setAgents(agentsData);
      setError(null);
    } catch (err) {
      console.error('Error fetching agents for topic:', err);
      setError('Failed to load agents');
      setAgents([]);
    } finally {
      setIsLoading(false);
    }
  }, [topicId]);

  /**
   * Handles disconnecting an agent from the topic
   */
  const handleDisconnect = async (agentId: string) => {
    if (!topicId) return;
    
    try {
      setIsLoading(true);
      await topicAgentService.removeAgentFromTopic(topicId, agentId);
      // Refresh the agents list after disconnecting
      await fetchAgents();
    } catch (err) {
      console.error('Error disconnecting agent from topic:', err);
      setError('Failed to disconnect agent');
    } finally {
      setIsLoading(false);
    }
  };

  // Fetch topic details and agents when component mounts or topicId changes
  useEffect(() => {
    if (topicId) {
      fetchTopicDetails();
      fetchAgents();
    } else {
      // Reset state when no topic is selected
      setTopic(null);
      setAgents([]);
      setError(null);
    }
  }, [topicId, fetchTopicDetails, fetchAgents]);

  // If no topic ID is provided, show a message
  if (!topicId) {
    return (
      <div className="p-6 bg-white dark:bg-gray-800 rounded-lg shadow-lg text-center">
        <p className="text-gray-600 dark:text-gray-300">
          Please select a topic from the Details tab first.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          {topic ? `Agents for ${topic.name}` : 'Topic Agents'}
        </h2>
      </div>

      {error && (
        <div className="p-4 bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-300 rounded-lg">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
        </div>
      ) : (
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden">
          {agents.length === 0 ? (
            <div className="p-6 text-center">
              <p className="text-gray-600 dark:text-gray-400">
                No agents are connected to this topic.
              </p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead className="bg-gray-50 dark:bg-gray-700">
                  <tr>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Agent
                    </th>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Description
                    </th>
                    <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  {agents.map((agent) => (
                    <tr key={agent.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div>
                            <div className="text-sm font-medium text-gray-900 dark:text-white">
                              {agent.name}
                            </div>
                            <div className="text-xs text-gray-500 dark:text-gray-400">
                              ID: {agent.id}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm text-gray-500 dark:text-gray-400 line-clamp-2">
                          {agent.description || 'No description'}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button
                          onClick={() => handleDisconnect(agent.id)}
                          className="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300 transition-colors"
                          disabled={isLoading}
                        >
                          Disconnect
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default TopicAgents;