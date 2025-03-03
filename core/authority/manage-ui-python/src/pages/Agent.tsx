import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import { AgentDetailsTab } from '../components/agents/AgentDetailsTab';
import { AgentPluginsTab } from '../components/agents/AgentPluginsTab';
import { AgentTopicsTab } from '../components/agents/AgentTopicsTab';
import { AgentCredentialsTab } from '../components/agents/AgentCredentialsTab';
import { AgentFormData } from '../components/agents/AgentDetailsForm';
import { agentService } from '../services/api/agentService';
import { Agent as AgentType } from '../types/Agent';

const tabs = [
  { id: 'details', label: 'Details' },
  { id: 'plugins', label: 'Plugins' },
  { id: 'topics', label: 'Topics' },
  { id: 'credentials', label: 'Credentials' },
];

export default function Agent() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState('details');
  const [selectedAgent, setSelectedAgent] = useState<AgentType | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  // Get agent ID from URL if available
  const agentId = searchParams.get('id');

  // Fetch agent details when ID changes
  useEffect(() => {
    /**
     * Fetches agent details if an ID is provided in the URL
     */
    const fetchAgentDetails = async () => {
      if (!agentId) {
        setSelectedAgent(null);
        return;
      }

      try {
        setIsLoading(true);
        const agent = await agentService.getAgentById(agentId);
        setSelectedAgent(agent);
      } catch (error) {
        console.error('Error fetching agent details:', error);
        setSelectedAgent(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchAgentDetails();
  }, [agentId]);

  /**
   * Handles saving agent data
   * @param data - The agent form data to save
   */
  const handleSave = (data: AgentFormData) => {
    console.log('Saving:', data);
    // Implement save logic
  };

  /**
   * Handles deleting an agent
   */
  const handleDelete = () => {
    console.log('Deleting agent');
    // Implement delete logic
  };

  /**
   * Handles selecting an agent from the list
   * @param id - The ID of the selected agent
   */
  const handleSelectAgent = (id: string) => {
    // Update URL with selected agent ID
    setSearchParams({ id });
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">
        {selectedAgent ? `Agent: ${selectedAgent.name}` : 'Agent Configuration'}
      </h1>
      
      <TabNavigation
        tabs={tabs}
        activeTab={activeTab}
        onTabChange={setActiveTab}
      />

      <div className="mt-6">
        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-500"></div>
          </div>
        ) : (
          <>
            {activeTab === 'details' && (
              <AgentDetailsTab 
                selectedAgent={selectedAgent}
                onSelectAgent={handleSelectAgent}
                onSave={handleSave}
                onDelete={handleDelete}
              />
            )}
            {activeTab === 'plugins' && <AgentPluginsTab agentId={agentId} />}
            {activeTab === 'topics' && <AgentTopicsTab agentId={agentId} />}
            {activeTab === 'credentials' && <AgentCredentialsTab agentId={agentId} />}
          </>
        )}
      </div>
    </div>
  );
}
