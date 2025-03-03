import { useState, useEffect } from 'react';
import { Agent, AgentFormData } from '../../types/Agent';
import { agentService } from '../../services/api/agentService';
import { hostService } from '../../services/api/hostService';
import { Host } from '../../types/Host';
import { Button } from '../common/Button';
import { AgentModal } from './AgentModal';

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
  
  // Edit modal state
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [agentToEdit, setAgentToEdit] = useState<Agent | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

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


  // Function to display host information
  const displayHostInfo = (hostId: string | null): string => {
    if (!hostId) return 'No host';
    return hosts[hostId]?.name || 'Unknown host';
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
      
      // Show success message
      setSuccessMessage('Agent deleted successfully!');
      
      // Refresh the agents list
      fetchAgents();
      
      // Call the onDeleteSuccess callback if provided
      if (onDeleteSuccess) {
        onDeleteSuccess();
      }
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
      
    } catch (err) {
      console.error('Error deleting agent:', err);
      setError('Failed to delete agent. Please try again.');
    } finally {
      setIsDeleting(false);
      setIsDeleteConfirmOpen(false);
      setAgentToDelete(null);
    }
  };

  // Function to open edit modal
  const handleEditClick = (agent: Agent, e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent triggering the row click
    setAgentToEdit(agent);
    setIsEditModalOpen(true);
  };

  // Function to handle updating an agent
  const handleUpdateAgent = async (formData: AgentFormData): Promise<void> => {
    if (!agentToEdit) return;
    
    try {
      // Make sure we're sending the correct data format with proper naming convention
      await agentService.updateAgent(agentToEdit.id, {
        name: formData.name,
        description: formData.description,
        persona: formData.persona || null,
        hostId: formData.hostId || null,
        executiveFunctionId: formData.executiveFunctionId || null,
        is_enabled: formData.is_enabled
      });
      
      // Show success message
      setSuccessMessage('Agent updated successfully!');
      
      // Refresh the agents list
      fetchAgents();
      
      // Close the modal
      setIsEditModalOpen(false);
      setAgentToEdit(null);
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
      
      // Reload the page to see the updates
      window.location.reload();
      
    } catch (err) {
      console.error('Error updating agent:', err);
      setError('Failed to update agent. Please try again.');
    }
  };

  return (
    <div className="space-y-4">
      {/* Success message */}
      {successMessage && (
        <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-4 animate-fadeIn">
          <p className="text-green-600 dark:text-green-400">{successMessage}</p>
        </div>
      )}

      {/* Error message */}
      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4">
          <p className="text-red-600 dark:text-red-400">{error}</p>
        </div>
      )}

      {/* Edit Modal */}
      {isEditModalOpen && agentToEdit && (
        <AgentModal
          isOpen={isEditModalOpen}
          onClose={() => setIsEditModalOpen(false)}
          onSave={handleUpdateAgent}
          initialData={{
            name: agentToEdit.name,
            description: agentToEdit.description,
            persona: agentToEdit.persona,
            hostId: agentToEdit.hostId,
            executiveFunctionId: agentToEdit.executiveFunctionId,
            is_enabled: agentToEdit.is_enabled
          }}
          isEditing={true}
        />
      )}

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && agentToDelete && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md">
            <div className="p-6">
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
                Confirm Delete
              </h3>
              <p className="text-gray-500 dark:text-gray-400 mb-6">
                Are you sure you want to delete the agent "{agentToDelete.name}"? This action cannot be undone.
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
      
      {isLoading ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-purple-500 dark:border-purple-400"></div>
        </div>
      ) : agents.length > 0 ? (
        <div className="overflow-x-auto bg-white dark:bg-gray-800 rounded-lg shadow border border-gray-200 dark:border-gray-700">
          {/* Desktop view */}
          <div className="hidden sm:block">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Agent
                  </th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Description
                  </th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Host
                  </th>
                  <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {agents.map((agent) => (
                  <tr 
                    key={agent.id} 
                    onClick={() => onSelectAgent(agent.id)}
                    className="cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="flex-shrink-0 h-10 w-10">
                          <img 
                            className="h-10 w-10 rounded-full object-cover border border-gray-200 dark:border-gray-700" 
                            src={agent.imageUrl || '/astra-avatar.png'} 
                            alt={agent.name} 
                          />
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-medium text-gray-900 dark:text-white">
                            {agent.name}
                          </div>
                          <div className="text-xs text-gray-500 dark:text-gray-400 flex items-center">
                            <svg 
                              className="mr-1 h-3 w-3 text-gray-400 dark:text-gray-500" 
                              xmlns="http://www.w3.org/2000/svg" 
                              fill="none" 
                              viewBox="0 0 24 24" 
                              stroke="currentColor"
                            >
                              <path 
                                strokeLinecap="round" 
                                strokeLinejoin="round" 
                                strokeWidth={2} 
                                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" 
                              />
                            </svg>
                            {new Date(agent.created_date).toLocaleDateString()}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900 dark:text-white">
                        {agent.description ? (
                          agent.description.length > 100 
                            ? `${agent.description.substring(0, 100)}...` 
                            : agent.description
                        ) : (
                          <span className="text-gray-500 dark:text-gray-400 italic">No description provided</span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex flex-col space-y-2">
                        <span className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          agent.is_enabled 
                            ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                            : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                        }`}>
                          {agent.is_enabled ? 'Enabled' : 'Disabled'}
                        </span>
                        {agent.is_connected && (
                          <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                            Connected
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      <div className="flex items-center">
                        <svg 
                          className="mr-1.5 h-4 w-4 text-gray-400 dark:text-gray-500" 
                          xmlns="http://www.w3.org/2000/svg" 
                          fill="none" 
                          viewBox="0 0 24 24" 
                          stroke="currentColor"
                        >
                          <path 
                            strokeLinecap="round" 
                            strokeLinejoin="round" 
                            strokeWidth={2} 
                            d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01" 
                          />
                        </svg>
                        {displayHostInfo(agent.hostId)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button 
                        onClick={(e) => handleEditClick(agent, e)}
                        className="text-purple-600 dark:text-purple-400 hover:text-purple-800 dark:hover:text-purple-300 transition-colors mr-4"
                      >
                        <span className="flex items-center">
                          <svg 
                            className="mr-1 h-4 w-4" 
                            xmlns="http://www.w3.org/2000/svg" 
                            fill="none" 
                            viewBox="0 0 24 24" 
                            stroke="currentColor"
                          >
                            <path 
                              strokeLinecap="round" 
                              strokeLinejoin="round" 
                              strokeWidth={2} 
                              d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" 
                            />
                          </svg>
                          Edit
                        </span>
                      </button>
                      <button 
                        onClick={(e) => handleDeleteClick(agent, e)}
                        className="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-300 transition-colors"
                      >
                        <span className="flex items-center">
                          <svg 
                            className="mr-1 h-4 w-4" 
                            xmlns="http://www.w3.org/2000/svg" 
                            fill="none" 
                            viewBox="0 0 24 24" 
                            stroke="currentColor"
                          >
                            <path 
                              strokeLinecap="round" 
                              strokeLinejoin="round" 
                              strokeWidth={2} 
                              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" 
                            />
                          </svg>
                          Delete
                        </span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile view */}
          <div className="sm:hidden divide-y divide-gray-200 dark:divide-gray-700">
            {agents.map((agent) => (
              <div 
                key={agent.id} 
                className="p-4 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                onClick={() => onSelectAgent(agent.id)}
              >
                <div className="flex items-center mb-3">
                  <img 
                    className="h-12 w-12 rounded-full object-cover border border-gray-200 dark:border-gray-700 mr-4" 
                    src={agent.imageUrl || '/astra-avatar.png'} 
                    alt={agent.name} 
                  />
                  <div>
                    <h3 className="font-medium text-gray-900 dark:text-white">{agent.name}</h3>
                    <div className="flex items-center text-xs text-gray-500 dark:text-gray-400 mt-1">
                      <svg 
                        className="mr-1 h-3 w-3 text-gray-400 dark:text-gray-500" 
                        xmlns="http://www.w3.org/2000/svg" 
                        fill="none" 
                        viewBox="0 0 24 24" 
                        stroke="currentColor"
                      >
                        <path 
                          strokeLinecap="round" 
                          strokeLinejoin="round" 
                          strokeWidth={2} 
                          d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" 
                        />
                      </svg>
                      Created: {new Date(agent.created_date).toLocaleDateString()}
                    </div>
                  </div>
                </div>
                
                <p className="text-sm text-gray-500 dark:text-gray-400 mb-3">
                  {agent.description ? (
                    agent.description.length > 120 
                      ? `${agent.description.substring(0, 120)}...` 
                      : agent.description
                  ) : (
                    <span className="italic">No description provided</span>
                  )}
                </p>
                
                <div className="flex justify-between items-center">
                  <div>
                    <div className="flex flex-wrap gap-2 mb-2">
                      <span className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${
                        agent.is_enabled 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                          : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                      }`}>
                        {agent.is_enabled ? 'Enabled' : 'Disabled'}
                      </span>
                      {agent.is_connected && (
                        <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                          Connected
                        </span>
                      )}
                    </div>
                    <div className="flex items-center text-xs text-gray-500 dark:text-gray-400">
                      <svg 
                        className="mr-1 h-3 w-3 text-gray-400 dark:text-gray-500" 
                        xmlns="http://www.w3.org/2000/svg" 
                        fill="none" 
                        viewBox="0 0 24 24" 
                        stroke="currentColor"
                      >
                        <path 
                          strokeLinecap="round" 
                          strokeLinejoin="round" 
                          strokeWidth={2} 
                          d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01" 
                        />
                      </svg>
                      Host: {displayHostInfo(agent.hostId)}
                    </div>
                  </div>
                  <div className="flex flex-col space-y-2">
                    <button 
                      onClick={(e) => handleEditClick(agent, e)}
                      className="text-purple-600 dark:text-purple-400 hover:text-purple-800 dark:hover:text-purple-300 transition-colors text-sm flex items-center justify-end"
                    >
                      <svg 
                        className="mr-1 h-4 w-4" 
                        xmlns="http://www.w3.org/2000/svg" 
                        fill="none" 
                        viewBox="0 0 24 24" 
                        stroke="currentColor"
                      >
                        <path 
                          strokeLinecap="round" 
                          strokeLinejoin="round" 
                          strokeWidth={2} 
                          d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" 
                        />
                      </svg>
                      Edit
                    </button>
                    <button 
                      onClick={(e) => handleDeleteClick(agent, e)}
                      className="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-300 transition-colors text-sm flex items-center justify-end"
                    >
                      <svg 
                        className="mr-1 h-4 w-4" 
                        xmlns="http://www.w3.org/2000/svg" 
                        fill="none" 
                        viewBox="0 0 24 24" 
                        stroke="currentColor"
                      >
                        <path 
                          strokeLinecap="round" 
                          strokeLinejoin="round" 
                          strokeWidth={2} 
                          d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" 
                        />
                      </svg>
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-8 text-center border border-gray-200 dark:border-gray-700">
          <div className="flex flex-col items-center">
            <svg 
              xmlns="http://www.w3.org/2000/svg" 
              className="h-16 w-16 text-gray-400 dark:text-gray-500 mb-4" 
              fill="none" 
              viewBox="0 0 24 24" 
              stroke="currentColor"
            >
              <path 
                strokeLinecap="round" 
                strokeLinejoin="round" 
                strokeWidth={1.5} 
                d="M17 8h2a2 2 0 012 2v6a2 2 0 01-2 2h-2v4l-4-4H9a1.994 1.994 0 01-1.414-.586m0 0L11 14h4a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2v4l.586-.586z" 
              />
            </svg>
            <p className="text-gray-600 dark:text-gray-300 text-lg">
              No agents configured yet
            </p>
            <p className="text-gray-500 dark:text-gray-400 mt-2">
              Create your first agent using the form
            </p>
          </div>
        </div>
      )}
    </div>
  );
}; 