import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Plugin } from '../../types/Plugin';
import { Agent } from '../../types/Agent';
import { agentService } from '../../services/api/agentService';
import { agentPluginService } from '../../services/api/agentPluginService';
import AgentList from './AgentList';
import PluginCard from './plugins/PluginCard';

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
  const [selectedAgent, setSelectedAgent] = useState<Agent | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [agent, setAgent] = useState<Agent | null>(null);
  const [assignedPlugins, setAssignedPlugins] = useState<Plugin[]>([]);
  const [availablePlugins, setAvailablePlugins] = useState<Plugin[]>([]);
  const [searchTerm, setSearchTerm] = useState('');

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

  // Fetch available plugins
  const fetchAvailablePlugins = useCallback(async () => {
    try {
      const plugins = await agentPluginService.getAvailablePlugins();
      setAvailablePlugins(plugins);
    } catch (error) {
      console.error('Error fetching available plugins:', error);
    }
  }, []);

  // Initial data fetch
  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      await Promise.all([
        fetchAgents(),
        fetchAgentDetails(),
        fetchAssignedPlugins(),
        fetchAvailablePlugins()
      ]);
      setIsLoading(false);
    };

    fetchData();
  }, [fetchAgents, fetchAgentDetails, fetchAssignedPlugins, fetchAvailablePlugins]);

  // Handle plugin toggle
  const handleTogglePlugin = async (pluginId: string) => {
    if (!contextAgentId) return;

    setIsLoading(true);
    try {
      const isAssigned = assignedPlugins.some(p => p.id === pluginId);

      if (isAssigned) {
        await agentPluginService.removePluginFromAgent(contextAgentId, pluginId);
      } else {
        await agentPluginService.addPluginToAgent(contextAgentId, pluginId);
      }

      // Refresh the plugin lists
      await fetchAssignedPlugins();
    } catch (error) {
      console.error('Error toggling plugin:', error);
    } finally {
      setIsLoading(false);
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
    </div>
  );
};