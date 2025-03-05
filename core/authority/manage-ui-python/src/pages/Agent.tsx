import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import AgentDetailsTab from '../components/agents/AgentDetailsTab';
import { AgentPluginsTab } from '../components/agents/AgentPluginsTab';
import AgentTopicsTab from '../components/agents/AgentTopicsTab';
import AgentCredentialsTab from '../components/agents/AgentCredentialsTab';

const tabs = [
  { id: 'details', label: 'Details' },
  { id: 'plugins', label: 'Plugins' },
  { id: 'topics', label: 'Topics' },
  { id: 'credentials', label: 'Credentials' },
];

/**
 * Agent page component that provides tab navigation for agent configuration
 * Supports both light and dark modes
 */
export default function Agent() {
  const [activeTab, setActiveTab] = useState('details');
  const [searchParams] = useSearchParams();
  
  // Get agent ID from URL if available
  const agentId = searchParams.get('id');

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">
        Agent Configuration
      </h1>
      
      <TabNavigation
        tabs={tabs}
        activeTab={activeTab}
        onTabChange={setActiveTab}
      />

      <div className="mt-6">
        {activeTab === 'details' && <AgentDetailsTab />}
        {activeTab === 'plugins' && agentId && <AgentPluginsTab agentId={agentId} />}
        {activeTab === 'topics' && agentId && <AgentTopicsTab agentId={agentId} />}
        {activeTab === 'credentials' && agentId && <AgentCredentialsTab agentId={agentId} />}
        
        {activeTab !== 'details' && !agentId && (
          <div className="p-6 bg-white dark:bg-gray-800 rounded-lg text-center shadow-lg">
            <p className="text-gray-600 dark:text-gray-300">Please select an agent from the Details tab first.</p>
          </div>
        )}
      </div>
    </div>
  );
}
