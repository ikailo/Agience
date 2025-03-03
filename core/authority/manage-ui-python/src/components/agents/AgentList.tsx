import { useState, useEffect } from 'react';
import { Agent } from '../../types/Agent';
import { agentService } from '../../services/api/agentService';
import { hostService } from '../../services/api/hostService';
import { Host } from '../../types/Host';
import { Button } from '../common/Button';

interface AgentListProps {
  onSelectAgent: (id: string) => void;
  onDeleteSuccess?: () => void;
}

export const AgentList: React.FC<AgentListProps> = ({ 
  onSelectAgent,
  onDeleteSuccess 
}) => {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [hosts, setHosts] = useState<Record<string, Host>>({});
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
  const [agentToDelete, setAgentToDelete] = useState<Agent | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Fetch agents and hosts when component mounts
  useEffect(() => {
    fetchAgents();
    fetchHosts();
  }, []);

  // Function to fetch all agents
  const fetchAgents = async () => {
    try {
      setIsLoading(true);
      const agentsData = await agentService.getAllAgents();
      console.log('Agents data:', agentsData);
      setAgents(agentsData);
      setError(null);
    } catch (err) {
      console.error('Error fetching agents:', err);
      setError('Failed to load agents. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  // Function to fetch all hosts and create a lookup map
  const fetchHosts = async () => {
    try {
      const hostsData = await hostService.getAllHosts();
      
      // Create a map of host ID to host object for easy lookup
      const hostsMap: Record<string, Host> = {};
      hostsData.forEach(host => {
        hostsMap[host.id] = host;
      });
      
      setHosts(hostsMap);
    } catch (err) {
      console.error('Error fetching hosts:', err);
    }
  };

  // Function to get host name from host ID
  const getHostName = (hostId: string | null): string => {
    if (!hostId) return 'No host';
    return hosts[hostId]?.name || 'Unknown host';
  };

  // Function to get host address from host ID
  const getHostAddress = (hostId: string | null): string => {
    if (!hostId) return '';
    return hosts[hostId]?.address || '';
  };

  // Function to display host information
  const displayHostInfo = (hostId: string | null): string => {
    if (!hostId) return 'No host';
    const host = hosts[hostId];
    if (!host) return 'Unknown host';
    return host.address ? `${host.name} (${host.address})` : host.name;
  };

  // Function to open delete confirmation
  const handleDeleteClick = (agent: Agent, e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent triggering the row click
    setAgentToDelete(agent);
    setIsDeleteConfirmOpen(true);
  };

  // Function to handle deleting an agent
  const handleDeleteAgent = async () => {
    if (!agentToDelete) return;
    
    try {
      setIsDeleting(true);
      setError(null);
      
      await agentService.deleteAgent(agentToDelete.id);
      
      // Refresh the agents list
      fetchAgents();
      
      // Call the onDeleteSuccess callback if provided
      if (onDeleteSuccess) {
        onDeleteSuccess();
      }
      
    } catch (err) {
      console.error('Error deleting agent:', err);
      setError('Failed to delete agent. Please try again.');
    } finally {
      setIsDeleting(false);
      setIsDeleteConfirmOpen(false);
      setAgentToDelete(null);
    }
  };

  // Function to handle editing an agent
  const handleEditClick = (agent: Agent, e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent triggering the row click
    onSelectAgent(agent.id);
  };

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-medium text-gray-900 dark:text-white">
        Your Agents
      </h2>
      
      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4">
          <p className="text-red-600 dark:text-red-400">{error}</p>
        </div>
      )}
      
      {isLoading ? (
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
        </div>
      ) : agents.length > 0 ? (
        <div className="space-y-4">
          {/* Desktop view */}
          <div className="hidden sm:block overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Agent
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Host
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {agents.map((agent) => (
                  <tr 
                    key={agent.id} 
                    onClick={() => onSelectAgent(agent.id)}
                    className="cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="flex-shrink-0 h-10 w-10">
                          <img 
                            className="h-10 w-10 rounded-full" 
                            src={agent.imageUrl || '/astra-avatar.png'} 
                            alt={agent.name} 
                          />
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-medium text-gray-900 dark:text-white">
                            {agent.name}
                          </div>
                          <div className="text-sm text-gray-500 dark:text-gray-400">
                            {agent.description.length > 50 
                              ? `${agent.description.substring(0, 50)}...` 
                              : agent.description}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                        agent.is_enabled 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                          : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                      }`}>
                        {agent.is_enabled ? 'Enabled' : 'Disabled'}
                      </span>
                      {agent.is_connected && (
                        <span className="ml-2 px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                          Connected
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {displayHostInfo(agent.host_id)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button 
                        onClick={(e) => handleEditClick(agent, e)}
                        className="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300 mr-4"
                      >
                        Edit
                      </button>
                      <button 
                        onClick={(e) => handleDeleteClick(agent, e)}
                        className="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile view */}
          <div className="sm:hidden space-y-4">
            {agents.map((agent) => (
              <div 
                key={agent.id} 
                className="bg-white dark:bg-gray-800 shadow rounded-lg p-4"
                onClick={() => onSelectAgent(agent.id)}
              >
                <div className="flex items-center">
                  <img 
                    className="h-10 w-10 rounded-full mr-4" 
                    src={agent.imageUrl || '/astra-avatar.png'} 
                    alt={agent.name} 
                  />
                  <div>
                    <h3 className="font-medium text-gray-900 dark:text-white">{agent.name}</h3>
                    <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                      {agent.description.length > 100 
                        ? `${agent.description.substring(0, 100)}...` 
                        : agent.description}
                    </p>
                  </div>
                </div>
                
                <div className="mt-4 flex items-center justify-between">
                  <div>
                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                      agent.is_enabled 
                        ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                        : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                    }`}>
                      {agent.is_enabled ? 'Enabled' : 'Disabled'}
                    </span>
                    {agent.is_connected && (
                      <span className="ml-2 px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                        Connected
                      </span>
                    )}
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      Host: {displayHostInfo(agent.host_id)}
                    </p>
                  </div>
                  <div className="flex space-x-4">
                    <button 
                      onClick={(e) => handleEditClick(agent, e)}
                      className="text-blue-600 dark:text-blue-400"
                    >
                      Edit
                    </button>
                    <button 
                      onClick={(e) => handleDeleteClick(agent, e)}
                      className="text-red-600 dark:text-red-400"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <p className="text-gray-500 dark:text-gray-400">
            No agents configured yet. Create your first agent using the form.
          </p>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md">
            <div className="p-6">
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
                Confirm Delete
              </h3>
              <p className="text-gray-500 dark:text-gray-400 mb-6">
                Are you sure you want to delete the agent "{agentToDelete?.name}"? This action cannot be undone.
              </p>
              <div className="flex justify-end space-x-3">
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => setIsDeleteConfirmOpen(false)}
                >
                  Cancel
                </Button>
                <Button
                  type="button"
                  variant="danger"
                  onClick={handleDeleteAgent}
                  disabled={isDeleting}
                >
                  {isDeleting ? 'Deleting...' : 'Delete'}
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}; 