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
   * Fetches a specific agent's details by ID
   */
  const fetchAgentDetails = async (id: string) => {
    try {
      setIsLoading(true);
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
    } catch (error) {
      console.error('Error fetching agent details:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // Fetch all agents and hosts on component mount
  useEffect(() => {
    fetchAgents();
    fetchHosts();
  }, []);

  // Fetch agent details when ID changes in URL
  useEffect(() => {
    if (agentId) {
      fetchAgentDetails(agentId);
    } else {
      setSelectedAgent(null);
      // Reset form data when no agent is selected
      setFormData({
        name: '',
        description: '',
        persona: null,
        hostId: null,
        executiveFunctionId: null,
        is_enabled: false
      });
    }
  }, [agentId]);

  /**
   * Handles selecting an agent from the list
   */
  const handleSelectAgent = (id: string) => {
    // Update URL with selected agent ID
    setSearchParams({ id });
  };

  /**
   * Handles creating a new agent
   */
  const handleCreateAgent = () => {
    // Clear form data and selected agent
    setFormData({
      name: '',
      description: '',
      persona: null,
      hostId: null,
      executiveFunctionId: null,
      is_enabled: false
    });
    setSelectedAgent(null);
    setSearchParams({});
  };

  /**
   * Handles form input changes
   */
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    
    // Handle checkbox inputs
    if (type === 'checkbox') {
      const checked = (e.target as HTMLInputElement).checked;
      setFormData(prev => ({ ...prev, [name]: checked }));
    } else {
      setFormData(prev => ({ ...prev, [name]: value === '' ? null : value }));
    }
  };

  /**
   * Handles saving agent data
   */
  const handleSave = async () => {
    try {
      setIsLoading(true);
      
      if (selectedAgent) {
        // Update existing agent
        await agentService.updateAgent(selectedAgent.id, formData);
      } else {
        // Create new agent
        const newAgent = await agentService.createAgent(formData);
        setSearchParams({ id: newAgent.id });
      }
      
      // Refresh agent list
      fetchAgents();
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
    if (!selectedAgent) return;
    
    if (window.confirm(`Are you sure you want to delete agent "${selectedAgent.name}"?`)) {
      try {
        setIsLoading(true);
        await agentService.deleteAgent(selectedAgent.id);
        
        // Clear selection and refresh list
        setSelectedAgent(null);
        setSearchParams({});
        fetchAgents();
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
          agents={agents}
          selectedAgentId={selectedAgent?.id || null}
          isLoading={isLoading}
          onSelectAgent={handleSelectAgent}
          onCreateAgent={handleCreateAgent}
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
        />
      </div>
    </div>
  );
}

export default AgentDetailsTab;
