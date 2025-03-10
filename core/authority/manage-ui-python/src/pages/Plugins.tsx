import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import PluginDetailsTab from '../components/plugins/PluginDetailsTab';
import PluginFunctionsTab from '../components/plugins/PluginFunctionsTab';

/**
 * Plugins page component with tab navigation
 */
const Plugins = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState<string>('details');
  const [selectedPluginId, setSelectedPluginId] = useState<string | null>(null);

  // Initialize state from URL parameters
  useEffect(() => {
    const tab = searchParams.get('tab');
    const pluginId = searchParams.get('id');
    
    if (tab) {
      setActiveTab(tab);
    }
    
    if (pluginId) {
      setSelectedPluginId(pluginId);
    }
  }, [searchParams]);

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

  // Define tabs
  const tabs = [
    { id: 'details', label: 'Details' },
    { id: 'functions', label: 'Functions' }
  ];

  return (
    <div className="flex flex-col h-full">
      <h1 className="text-2xl font-semibold text-white mb-6">
        Plugin Configuration
      </h1>
      
      <div className="flex flex-col h-full">
        <div className="mb-6">
          <TabNavigation
            tabs={tabs}
            activeTab={activeTab}
            onTabChange={handleTabChange}
          />
        </div>
        
        <div className="flex-grow">
          {activeTab === 'details' && (
            <PluginDetailsTab
              onSelectPlugin={handleSelectPlugin}
              selectedPluginId={selectedPluginId}
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
