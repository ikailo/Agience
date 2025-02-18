import { useState } from 'react';
import { TabNavigation } from '../components/common/TabNavigation';
import { AgentDetailsForm, AgentFormData } from '../components/agents/AgentDetailsForm';
import { AgentPluginsTab } from '../components/agents/AgentPluginsTab';
import { AgentTopicsTab } from '../components/agents/AgentTopicsTab';
import { AgentCredentialsTab } from '../components/agents/AgentCredentialsTab';

const tabs = [
  { id: 'details', label: 'Details' },
  { id: 'plugins', label: 'Plugins' },
  { id: 'topics', label: 'Topics' },
  { id: 'credentials', label: 'Credentials' },
];

export default function Agent() {
  const [activeTab, setActiveTab] = useState('details');

  const handleSave = (data: AgentFormData) => {
    console.log('Saving:', data);
    // Implement save logic
  };

  const handleDelete = () => {
    console.log('Deleting agent');
    // Implement delete logic
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Agent Configuration</h1>
      
      <TabNavigation
        tabs={tabs}
        activeTab={activeTab}
        onTabChange={setActiveTab}
      />

      <div className="mt-6">
        {activeTab === 'details' && (
          <AgentDetailsForm
            onSave={handleSave}
            onDelete={handleDelete}
          />
        )}
        {activeTab === 'plugins' && <AgentPluginsTab />}
        {activeTab === 'topics' && <AgentTopicsTab />}
        {activeTab === 'credentials' && <AgentCredentialsTab />}
      </div>
    </div>
  );
}
