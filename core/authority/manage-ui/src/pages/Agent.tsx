import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import AgentDetailsTab from '../components/agents/AgentDetailsTab';
import { AgentPluginsTab } from '../components/agents/AgentPluginsTab';
import AgentTopicsTab from '../components/agents/AgentTopicsTab';
import AgentCredentialsTab from '../components/agents/AgentCredentialsTab';

const tabs = [
  { id: 'Agents', label: 'Agents' },
  { id: 'plugins', label: 'Plugins' },
  { id: 'topics', label: 'Topics' },
  { id: 'credentials', label: 'Credentials' },
];

/**
 * Agent page component that provides tab navigation for agent configuration
 * Supports both light and dark modes
 */
export default function Agent() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState('Agents');
  
  // Get agent ID and tab from URL if available
  const agentId = searchParams.get('id');
  
  // Initialize state from URL parameters
  useEffect(() => {
    const tab = searchParams.get('tab');
    if (tab) {
      setActiveTab(tab);
    }
  }, [searchParams]);

  // Handle tab change
  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    
    // Update URL parameters
    const newParams = new URLSearchParams(searchParams);
    newParams.set('tab', tab);
    setSearchParams(newParams);
  };

  return (
    <div className="space-y-6">
      {/* <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">
        Agent Configuration
      </h1> */}
      
      <TabNavigation
        tabs={tabs}
        activeTab={activeTab}
        onTabChange={handleTabChange}
      />

      <div className="mt-6">
        {activeTab === 'Agents' && <AgentDetailsTab />}
        {activeTab === 'plugins' && <AgentPluginsTab />}
        {activeTab === 'topics' && <AgentTopicsTab />}
        {activeTab === 'credentials' && <AgentCredentialsTab agentId={agentId || undefined} />}
        
        {/* {activeTab !== 'Agents' && !agentId && (
          <div className="p-6 bg-white dark:bg-gray-800 rounded-lg text-center shadow-lg">
            <p className="text-gray-600 dark:text-gray-300">Please select an agent from the Agents tab first.</p>
          </div>
        )} */}
      </div>
    </div>
  );
}
