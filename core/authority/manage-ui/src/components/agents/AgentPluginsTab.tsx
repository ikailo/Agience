import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Plugin } from '../../types/Plugin';
import { Agent } from '../../types/Agent';
import { agentService } from '../../services/api/agentService';
import { agentPluginService } from '../../services/api/agentPluginService';
import AgentList from './AgentList';
import PluginCard from './plugins/PluginCard';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';

interface AgentPluginsTabProps {
  agentId?: string;
}

/**
 * AgentPluginsTab component that displays and manages plugins for an agent
 */
export const AgentPluginsTab: React.FC<AgentPluginsTabProps> = ({ agentId: propAgentId }) => {
  const [searchParams, setSearchParams] = useSearchParams();
  const urlAgentId = searchParams.get('id');
  
  // Use the prop agentId if provided, otherwise use the URL parameter
  const contextAgentId = propAgentId || urlAgentId;

  const [agents, setAgents] = useState<Agent[]>([]);
  const [, setSelectedAgent] = useState<Agent | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [agent, setAgent] = useState<Agent | null>(null);
  const [assignedPlugins, setAssignedPlugins] = useState<Plugin[]>([]);
  const [availablePlugins, setAvailablePlugins] = useState<Plugin[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [notification, setNotification] = useState<{ isOpen: boolean; title: string; message: string; type: 'success' | 'error' | 'info' | 'warning' }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'info'
  });
  const [confirmation, setConfirmation] = useState<{ isOpen: boolean; pluginId: string; pluginName: string }>({
    isOpen: false,
    pluginId: '',
    pluginName: ''
  });

  /*
  // Fetch all agents
  const fetchAgents = useCallback(async () => {
    try {
      const agentsData = await agentService.getAllAgents();
      setAgents(agentsData);
    } catch (error) {
      console.error('Error fetching agents:', error);
    }
  }, []);
*/

  // Fetch assigned plugins
  const fetchAssignedPlugins = useCallback(async () => {
    if (!contextAgentId) return;

    try {
      const plugins = await agentPluginService.getAgentPlugins(contextAgentId);
      setAssignedPlugins(plugins);
    } catch (error) {
      console.error('Error fetching assigned plugins:', error);
    }
  }, [contextAgentId]);

  // Initial data fetch
  useEffect(() => {
    const fetchData = async () => {
      if (!contextAgentId) {
        // If no agent is selected, just fetch the list of agents
        setIsLoading(true);
        try {
          const agentsData = await agentService.getAllAgents();
          setAgents(agentsData);
        } catch (error) {
          console.error('Error fetching agents:', error);
          showNotification('Error', 'Failed to load agents', 'error');
        } finally {
          setIsLoading(false);
        }
        return;
      }
      
      setIsLoading(true);
      try {
        // Fetch data in parallel to reduce loading time
        const [agentsData, agentData, assignedPluginsData, availablePluginsData] = await Promise.all([
          agentService.getAllAgents(),
          agentService.getAgentById(contextAgentId),
          agentPluginService.getAgentPlugins(contextAgentId),
          agentPluginService.getAvailablePlugins()
        ]);
        
        setAgents(agentsData);
        setAgent(agentData);
        setSelectedAgent(agentData);
        setAssignedPlugins(assignedPluginsData);
        setAvailablePlugins(availablePluginsData);
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
   * Shows the confirmation modal for plugin removal
   */
  const showRemoveConfirmation = (pluginId: string, pluginName: string) => {
    setConfirmation({ isOpen: true, pluginId, pluginName });
  };

  /**
   * Closes the confirmation modal
   */
  const closeConfirmation = () => {
    setConfirmation(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Handles the actual removal of a plugin after confirmation
   */
  const handleRemovePlugin = async () => {
    if (!contextAgentId || !confirmation.pluginId) return;

    setIsLoading(true);
    try {
      await agentPluginService.removePluginFromAgent(contextAgentId, confirmation.pluginId);
      await fetchAssignedPlugins();
      showNotification(
        'Plugin Removed',
        `Successfully removed plugin "${confirmation.pluginName}" from the agent.`,
        'success'
      );
    } catch (error) {
      console.error('Error removing plugin:', error);
      showNotification(
        'Error',
        `Failed to remove plugin "${confirmation.pluginName}". Please try again.`,
        'error'
      );
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  /**
   * Handles adding a plugin to the agent
   */
  const handleAddPlugin = async (pluginId: string) => {
    if (!contextAgentId) return;

    setIsLoading(true);
    try {
      await agentPluginService.addPluginToAgent(contextAgentId, pluginId);
      await fetchAssignedPlugins();
      const plugin = availablePlugins.find(p => p.id === pluginId);
      showNotification(
        'Plugin Added',
        `Successfully added plugin "${plugin?.name}" to the agent.`,
        'success'
      );
    } catch (error) {
      console.error('Error adding plugin:', error);
      const plugin = availablePlugins.find(p => p.id === pluginId);
      showNotification(
        'Error',
        `Failed to add plugin "${plugin?.name}". Please try again.`,
        'error'
      );
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Toggles a plugin's assignment to the agent
   */
  const handleTogglePlugin = (pluginId: string) => {
    const isAssigned = assignedPlugins.some(p => p.id === pluginId);
    
    if (isAssigned) {
      const plugin = assignedPlugins.find(p => p.id === pluginId);
      if (plugin) {
        showRemoveConfirmation(pluginId, plugin.name);
      }
    } else {
      handleAddPlugin(pluginId);
    }
  };

  // Filter plugins based on search term
  const filterPlugins = (plugins: Plugin[]) => {
    if (!searchTerm) return plugins;
    
    const term = searchTerm.toLowerCase();
    return plugins.filter(plugin => 
      plugin.name.toLowerCase().includes(term) ||
      plugin.description.toLowerCase().includes(term)
    );
  };

  // Get assigned plugin IDs for quick lookup
  const assignedPluginIds = assignedPlugins.map(p => p.id);

  // Filter available plugins to exclude already assigned ones
  const unassignedPlugins = availablePlugins.filter(plugin => !assignedPluginIds.includes(plugin.id));

  // Apply search filter
  const filteredAssignedPlugins = filterPlugins(assignedPlugins);
  const filteredUnassignedPlugins = filterPlugins(unassignedPlugins);

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
          <div className="flex flex-wrap justify-between items-center gap-3">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
              {agent ? `Plugins for ` : 'Agent Plugins'}
              {agent && <span className="dark:text-indigo-500 font-bold text-xl text-indigo-700">{agent.name}</span>}
            </h2>
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
                  {/* Search Input */}
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                      </svg>
                    </div>
                    <input
                      type="text"
                      placeholder="Search plugins..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="w-full pl-10 pr-4 py-2 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    />
                  </div>

                  {/* Assigned Plugins Section */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
                      Assigned Plugins ({filteredAssignedPlugins.length})
                    </h3>
                    <div className="grid grid-cols-1 gap-4">
                      {filteredAssignedPlugins.map(plugin => (
                        <PluginCard
                          key={plugin.id}
                          plugin={plugin}
                          isAssigned={true}
                          onToggle={handleTogglePlugin}
                        />
                      ))}
                      {filteredAssignedPlugins.length === 0 && (
                        <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                          <p className="text-gray-600 dark:text-gray-400">
                            No plugins assigned to this agent yet.
                          </p>
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Available Plugins Section */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
                      Available Plugins ({filteredUnassignedPlugins.length})
                    </h3>
                    <div className="grid grid-cols-1 gap-4">
                      {filteredUnassignedPlugins.map(plugin => (
                        <PluginCard
                          key={plugin.id}
                          plugin={plugin}
                          isAssigned={false}
                          onToggle={handleTogglePlugin}
                        />
                      ))}
                      {filteredUnassignedPlugins.length === 0 && (
                        <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                          <p className="text-gray-600 dark:text-gray-400">
                            No additional plugins available.
                          </p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
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
        onConfirm={handleRemovePlugin}
        title="Remove Plugin"
        message={`Are you sure you want to remove the plugin "${confirmation.pluginName}" from this agent?`}
        confirmText="Remove"
        cancelText="Cancel"
        type="danger"
        isLoading={isLoading}
      />
    </div>
  );
};