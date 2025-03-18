import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { agentService } from '../../services/api/agentService';
import { hostService } from '../../services/api/hostService';
import { Agent, AgentFormData } from '../../types/Agent';
import { Host } from '../../types/Host';
import AgentList from './AgentList';
import AgentForm from './AgentForm';

/**
 * AgentDetailsTab component that orchestrates the agent management UI
 */
function AgentDetailsTab() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [agents, setAgents] = useState<Agent[]>([]);
  const [hosts, setHosts] = useState<Host[]>([]);
  const [selectedAgent, setSelectedAgent] = useState<Agent | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [tempAgentId, setTempAgentId] = useState<string | null>(null);
  const [formData, setFormData] = useState<AgentFormData>({
    name: '',
    description: '',
    persona: null,
    hostId: null,
    executiveFunctionId: null,
    is_enabled: false
  });

  // Get agent ID from URL if available
  const agentId = searchParams.get('id');

  /**
   * Fetches all available agents from the API
   */
  const fetchAgents = async () => {
    try {
      setIsLoading(true);
      const agentList = await agentService.getAllAgents();
      setAgents(agentList);
    } catch (error) {
      console.error('Error fetching agents:', error);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Fetches all available hosts from the API
   */
  const fetchHosts = async () => {
    try {
      const hostList = await hostService.getAllHosts();
      setHosts(hostList);
    } catch (error) {
      console.error('Error fetching hosts:', error);
    }
  };

  /**
   * Fetches details for a specific agent by ID
   */
  const fetchAgentDetails = async (id: string) => {
    try {
      const agent = await agentService.getAgentById(id);
      setSelectedAgent(agent);
      
      // Update form data with agent details
      setFormData({
        name: agent.name,
        description: agent.description,
        persona: agent.persona,
        hostId: agent.hostId,
        executiveFunctionId: agent.executiveFunctionId,
        is_enabled: agent.is_enabled
      });
      
      // Update URL with agent ID
      setSearchParams({ id: agent.id });
    } catch (error) {
      console.error('Error fetching agent details:', error);
    }
  };

  // Initial data fetch
  useEffect(() => {
    fetchAgents();
    fetchHosts();
  }, []);

  // Fetch agent details when ID is available in URL
  useEffect(() => {
    if (agentId && !tempAgentId) {
      fetchAgentDetails(agentId);
    }
  }, [agentId]);

  /**
   * Handles selecting an agent from the list
   */
  const handleSelectAgent = (id: string) => {
    // Clear temp agent if exists
    if (tempAgentId) {
      setTempAgentId(null);
    }
    
    fetchAgentDetails(id);
  };

  /**
   * Creates a temporary agent for the form
   */
  const handleCreateAgent = () => {
    // Generate a temporary ID
    const tempId = `temp-${Date.now()}`;
    setTempAgentId(tempId);
    
    // Create a temporary agent object with required fields
    const tempAgent: Agent = {
      id: tempId,
      name: 'New Agent',
      description: '',
      persona: null,
      hostId: null,
      executiveFunctionId: null,
      is_enabled: false,
      created_date: new Date().toISOString(),
      autoStartFunctionId: null,
      onAutoStartFunctionComplete: null,
      owner_id: '',
      owner: {
        id: '',
        name: '',
        created_date: new Date().toISOString()
      },
      host: null,
      executiveFunction: null,
      autoStartFunction: null,
      is_connected: false,
      topics: [],
      plugins: []
    };
    
    // Set as selected agent
    setSelectedAgent(tempAgent);
    
    // Reset form data
    setFormData({
      name: '',
      description: '',
      persona: null,
      hostId: null,
      executiveFunctionId: null,
      is_enabled: false
    });
    
    // Clear URL parameter
    setSearchParams({});
  };

  /**
   * Handles form input changes
   */
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    
    // Handle checkbox inputs
    if (type === 'checkbox') {
      const checkbox = e.target as HTMLInputElement;
      setFormData(prev => ({
        ...prev,
        [name]: checkbox.checked
      }));
      
      console.log(`Checkbox ${name} changed to:`, checkbox.checked);
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: value
      }));
      
      console.log(`Field ${name} changed to:`, value);
    }
  };

  /**
   * Handles saving the agent (create or update)
   */
  const handleSave = async () => {
    try {
      setIsLoading(true);
      
      let savedAgent;
      
      if (selectedAgent && !tempAgentId) {
        // Update existing agent
        savedAgent = await agentService.updateAgent(selectedAgent.id, formData);
      } else {
        // Create new agent
        savedAgent = await agentService.createAgent(formData);
      }
      
      // Clear temp agent ID if exists
      if (tempAgentId) {
        setTempAgentId(null);
      }
      
      // Refresh agent list
      await fetchAgents();
      
      // Select the saved agent
      await fetchAgentDetails(savedAgent.id);
      
    } catch (error) {
      console.error('Error saving agent:', error);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles deleting an agent
   */
  const handleDelete = async () => {
    if (!selectedAgent || tempAgentId) return;
    
    if (window.confirm('Are you sure you want to delete this agent?')) {
      try {
        setIsLoading(true);
        await agentService.deleteAgent(selectedAgent.id);
        
        // Clear selected agent
        setSelectedAgent(null);
        setFormData({
          name: '',
          description: '',
          persona: null,
          hostId: null,
          executiveFunctionId: null,
          is_enabled: false
        });
        
        // Clear URL parameter
        setSearchParams({});
        
        // Refresh agent list
        await fetchAgents();
      } catch (error) {
        console.error('Error deleting agent:', error);
      } finally {
        setIsLoading(false);
      }
    }
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
      <div className="lg:col-span-1">
        <AgentList
          agents={tempAgentId ? [...agents, selectedAgent as Agent] : agents}
          selectedAgentId={selectedAgent?.id || null}
          isLoading={isLoading}
          onSelectAgent={handleSelectAgent}
          onCreateAgent={handleCreateAgent}
          hasTempAgent={!!tempAgentId}
        />
      </div>
      
      <div className="lg:col-span-3">
        <AgentForm
          formData={formData}
          selectedAgent={selectedAgent}
          isLoading={isLoading}
          hosts={hosts}
          onChange={handleInputChange}
          onSave={handleSave}
          onDelete={handleDelete}
          isTempAgent={!!tempAgentId}
        />
      </div>
    </div>
  );
}

export default AgentDetailsTab;
