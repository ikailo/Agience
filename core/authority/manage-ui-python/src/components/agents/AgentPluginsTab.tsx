import React, { useState, useEffect } from 'react';
import { agentService } from '../../services/api/agentService';
import { agentPluginService } from '../../services/api/agentPluginService';
import { pluginService } from '../../services/api/pluginService';
import { Plugin } from '../../types/Plugin';
import { Agent } from '../../types/Agent';
import PluginCard from './plugins/PluginCard';

interface AgentPluginsTabProps {
  agentId: string;
}

/**
 * AgentPluginsTab component that displays and manages plugins for an agent
 */
export const AgentPluginsTab: React.FC<AgentPluginsTabProps> = ({ agentId }) => {
  const [agent, setAgent] = useState<Agent | null>(null);
  const [assignedPlugins, setAssignedPlugins] = useState<Plugin[]>([]);
  const [availablePlugins, setAvailablePlugins] = useState<Plugin[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');

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
   * Fetches all plugins assigned to the agent
   */
  const fetchAssignedPlugins = async () => {
    try {
      setIsLoading(true);
      const plugins = await agentPluginService.getAgentPlugins(agentId);
      setAssignedPlugins(plugins);
    } catch (error) {
      console.error('Error fetching assigned plugins:', error);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Fetches all available plugins
   */
  const fetchAvailablePlugins = async () => {
    try {
      const plugins = await pluginService.getAllPlugins();
      setAvailablePlugins(plugins);
    } catch (error) {
      console.error('Error fetching available plugins:', error);
    }
  };

  // Fetch data on component mount
  useEffect(() => {
    let isMounted = true; // Track if the component is mounted

    const fetchData = async () => {
      if (isMounted) {
        await fetchAgentDetails();
        await fetchAssignedPlugins();
        await fetchAvailablePlugins();
      }
    };

    fetchData();

    // Cleanup function to set isMounted to false when the component unmounts
    return () => {
      isMounted = false;
    };
  }, [agentId]);

  /**
   * Toggles a plugin's assignment to the agent
   * @param pluginId - The ID of the plugin to toggle
   */
  const handleTogglePlugin = async (pluginId: string) => {
    try {
      setIsLoading(true);
      
      // Check if the plugin is already assigned
      const isAssigned = assignedPlugins.some(plugin => plugin.id === pluginId);
      
      if (isAssigned) {
        // Remove the plugin from the agent
        await agentPluginService.removePluginFromAgent(agentId, pluginId);
      } else {
        // Add the plugin to the agent
        await agentPluginService.addPluginToAgent(agentId, pluginId);
      }
      
      // Refresh the assigned plugins
      fetchAssignedPlugins();
    } catch (error) {
      console.error('Error toggling plugin:', error);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Filters plugins based on search term
   * @param plugins - The plugins to filter
   * @returns Filtered plugins
   */
  const filterPlugins = (plugins: Plugin[]) => {
    if (!searchTerm) return plugins;
    
    return plugins.filter(plugin => 
      plugin.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      plugin.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
      plugin.provider.toLowerCase().includes(searchTerm.toLowerCase())
    );
  };

  // Get the IDs of assigned plugins for easy lookup
  const assignedPluginIds = assignedPlugins.map(plugin => plugin.id);
  
  // Filter available plugins to exclude already assigned ones
  const unassignedPlugins = availablePlugins.filter(plugin => !assignedPluginIds.includes(plugin.id));

  // Apply search filter
  const filteredAssignedPlugins = filterPlugins(assignedPlugins);
  const filteredUnassignedPlugins = filterPlugins(unassignedPlugins);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap justify-between items-center gap-3">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          {agent ? `Plugins for ` : 'Agent Plugins'}
          {agent && <span className="dark:text-indigo-500 font-bold text-xl text-indigo-700">{agent.name}</span>}
        </h2>
      </div>

      {/* Search box */}
      {/* <div className="relative">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <svg className="h-5 w-5 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <input
          type="text"
          placeholder="Search plugins..."
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
          {/* Assigned Plugins Section */}
          <div>
            <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
              Assigned Plugins ({filteredAssignedPlugins.length})
            </h3>
            {filteredAssignedPlugins.length === 0 ? (
              <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                <p className="text-gray-600 dark:text-gray-400">
                  {searchTerm ? 'No matching assigned plugins found.' : 'No plugins assigned to this agent yet.'}
                </p>
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredAssignedPlugins.map(plugin => (
                  <PluginCard
                    key={plugin.id}
                    plugin={plugin}
                    isAssigned={true}
                    onToggle={handleTogglePlugin}
                  />
                ))}
              </div>
            )}
          </div>

          {/* Available Plugins Section */}
          <div>
            <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
              Available Plugins ({filteredUnassignedPlugins.length})
            </h3>
            {filteredUnassignedPlugins.length === 0 ? (
              <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                <p className="text-gray-600 dark:text-gray-400">
                  {searchTerm ? 'No matching available plugins found.' : 'All available plugins are already assigned.'}
                </p>
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredUnassignedPlugins.map(plugin => (
                  <PluginCard
                    key={plugin.id}
                    plugin={plugin}
                    isAssigned={false}
                    onToggle={handleTogglePlugin}
                  />
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};