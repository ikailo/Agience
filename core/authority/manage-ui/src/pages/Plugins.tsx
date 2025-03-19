import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import PluginDetailsTab from '../components/plugins/PluginDetailsTab';
import PluginFunctionsTab from '../components/plugins/PluginFunctionsTab';
import PluginList from '../components/plugins/PluginList';
import { pluginService } from '../services/api/pluginService';
import { Plugin } from '../types/Plugin';

/**
 * Plugins page component with tab navigation
 */
const Plugins = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState<string>('details');
  const [selectedPluginId, setSelectedPluginId] = useState<string | null>(null);
  const [plugins, setPlugins] = useState<Plugin[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [hasTempPlugin, setHasTempPlugin] = useState<boolean>(false);

  // Fetch plugins
  const fetchPlugins = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await pluginService.getAllPlugins();
      setPlugins(data);
    } catch (error) {
      console.error('Error fetching plugins:', error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initialize state from URL parameters and fetch plugins
  useEffect(() => {
    const tab = searchParams.get('tab');
    const pluginId = searchParams.get('id');
    
    if (tab) {
      setActiveTab(tab);
    }
    
    if (pluginId) {
      setSelectedPluginId(pluginId);
    }

    fetchPlugins();
  }, [searchParams, fetchPlugins]);

  /**
   * Handles tab change
   */
  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    
    // Update URL parameters
    const newParams = new URLSearchParams(searchParams);
    newParams.set('tab', tab);
    setSearchParams(newParams);
  };

  /**
   * Handles plugin selection
   */
  const handleSelectPlugin = (id: string, switchToFunctions: boolean = false) => {
    setSelectedPluginId(id);
    
    // Update URL parameters
    const newParams = new URLSearchParams(searchParams);
    
    if (id) {
      // Set the ID parameter
      newParams.set('id', id);
      
      // Only switch to functions tab if explicitly requested
      if (switchToFunctions) {
        setActiveTab('functions');
        newParams.set('tab', 'functions');
      }
    } else {
      // Remove the ID parameter and switch to details tab
      newParams.delete('id');
      setActiveTab('details');
      newParams.set('tab', 'details');
    }
    
    setSearchParams(newParams);
  };

  /**
   * Handles creating a new plugin
   */
  const handleCreatePlugin = () => {
    // Clear selection and switch to details tab
    handleSelectPlugin('', false);
    // Open the form in the details tab
    setHasTempPlugin(true);
  };

  /**
   * Callback for when a plugin is created, updated or deleted
   */
  const handlePluginChange = useCallback(async () => {
    // Refresh the plugins list
    await fetchPlugins();
    setHasTempPlugin(false);
  }, [fetchPlugins]);

  // Define tabs
  const tabs = [
    { id: 'details', label: 'Details' },
    { id: 'functions', label: 'Functions' }
  ];

  return (
    <div className="flex flex-col h-full">
      {/* Tabs at the top */}
      <div className="mb-6">
        <TabNavigation
          tabs={tabs}
          activeTab={activeTab}
          onTabChange={handleTabChange}
        />
      </div>
      
      {/* Content area with list and details */}
      <div className="flex flex-col md:flex-row h-full gap-6">
        {/* Left sidebar with plugin list */}
        <div className="w-full md:w-1/4 mb-6 md:mb-0">
          <PluginList
            plugins={plugins}
            selectedPluginId={selectedPluginId}
            isLoading={isLoading}
            onSelectPlugin={(id) => handleSelectPlugin(id, false)}
            onCreatePlugin={handleCreatePlugin}
            hasTempPlugin={hasTempPlugin}
          />
        </div>
        
        {/* Main content area */}
        <div className="w-full md:w-3/4">
          {activeTab === 'details' && (
            <PluginDetailsTab
              onSelectPlugin={handleSelectPlugin}
              selectedPluginId={selectedPluginId}
              onPluginChange={handlePluginChange}
              isCreatingNew={hasTempPlugin}
              onCancelCreate={() => setHasTempPlugin(false)}
            />
          )}
          
          {activeTab === 'functions' && (
            <PluginFunctionsTab
              pluginId={selectedPluginId || ''}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default Plugins;
