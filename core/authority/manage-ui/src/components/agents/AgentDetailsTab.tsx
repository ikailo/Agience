import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { agentService } from '../../services/api/agentService';
import { hostService } from '../../services/api/hostService';
import { Agent, AgentFormData } from '../../types/Agent';
import { Host } from '../../types/Host';
import AgentList from './AgentList';
import AgentForm from './AgentForm';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';

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

  // Modal states
  const [notification, setNotification] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'info'
  });

  const [confirmation, setConfirmation] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    type: 'danger' | 'warning' | 'info';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'danger'
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
        hostId: agent.hostId || null,
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
    const fetchInitialData = async () => {
      setIsLoading(true);
      try {
        // Fetch agents and hosts in parallel
        const [agentList, hostList] = await Promise.all([
          agentService.getAllAgents(),
          hostService.getAllHosts()
        ]);
        
        setAgents(agentList);
        setHosts(hostList);
        
        // If there's an agent ID in the URL, fetch its details
        if (agentId && !tempAgentId) {
          const agent = await agentService.getAgentById(agentId);
          setSelectedAgent(agent);
          
          // Update form data with agent details
          setFormData({
            name: agent.name,
            description: agent.description,
            persona: agent.persona,
            hostId: agent.hostId || null,
            executiveFunctionId: agent.executiveFunctionId,
            is_enabled: agent.is_enabled
          });
        }
      } catch (error) {
        console.error('Error fetching initial data:', error);
        showNotification('Error', 'Failed to load data', 'error');
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchInitialData();
  }, [agentId, tempAgentId]);

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
   * Shows a notification modal
   */
  const showNotification = (title: string, message: string, type: 'success' | 'error' | 'info' | 'warning') => {
    setNotification({
      isOpen: true,
      title,
      message,
      type
    });
  };

  /**
   * Closes the notification modal
   */
  const closeNotification = () => {
    setNotification(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Shows a confirmation modal for deleting an agent
   */
  const showDeleteConfirmation = (agent: Agent) => {
    setConfirmation({
      isOpen: true,
      title: 'Delete Agent',
      message: `Are you sure you want to delete the agent "${agent.name}"? This action cannot be undone.`,
      type: 'danger'
    });
  };

  /**
   * Closes the confirmation modal
   */
  const closeConfirmation = () => {
    setConfirmation(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Handles saving the agent (create or update)
   */
  const handleSave = async () => {
    try {
      setIsLoading(true);
      
      // Validate form data
      if (!formData.name.trim()) {
        showNotification('Error', 'Agent name is required', 'error');
        return;
      }
      
      let savedAgent;
      
      if (selectedAgent && !tempAgentId) {
        // Update existing agent
        savedAgent = await agentService.updateAgent(selectedAgent.id, formData);
        showNotification('Success', 'Agent updated successfully', 'success');
      } else {
        // Create new agent
        savedAgent = await agentService.createAgent(formData);
        showNotification('Success', 'Agent created successfully', 'success');
      }
      
      // Clear temp agent ID if exists
      if (tempAgentId) {
        setTempAgentId(null);
      }
      
      // Refresh agent list and select the saved agent
      const [updatedAgents] = await Promise.all([
        agentService.getAllAgents(),
        fetchAgentDetails(savedAgent.id)
      ]);
      
      setAgents(updatedAgents);
      
    } catch (error) {
      console.error('Error saving agent:', error);
      
      // Show more specific error message
      let errorMessage = 'Failed to save agent';
      
      if (error instanceof Error) {
        if (error.message.includes('validation')) {
          errorMessage = 'Validation error: Please check all fields';
        } else if (error.message.includes('already exists')) {
          errorMessage = 'An agent with this name already exists';
        } else {
          errorMessage = `Failed to save agent: ${error.message}`;
        }
      }
      
      showNotification('Error', errorMessage, 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles deleting an agent
   */
  const handleDelete = () => {
    if (!selectedAgent || tempAgentId) return;
    showDeleteConfirmation(selectedAgent);
  };

  /**
   * Handles confirming agent deletion
   */
  const handleConfirmDelete = async () => {
    if (!selectedAgent) return;
    
    try {
      setIsLoading(true);
      
      // Check if agent has plugins or topics
      if ((selectedAgent.plugins && selectedAgent.plugins.length > 0) || 
          (selectedAgent.topics && selectedAgent.topics.length > 0)) {
        
        // Show warning notification
        showNotification(
          'Warning',
          `This agent has ${selectedAgent.plugins.length} plugins and ${selectedAgent.topics.length} topics attached. These will be removed when deleting the agent.`,
          'warning'
        );
      }
      
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
      
      // Show success notification
      showNotification('Success', 'Agent deleted successfully', 'success');
      
    } catch (error) {
      console.error('Error deleting agent:', error);
      
      // Show more specific error message
      let errorMessage = 'Failed to delete agent';
      
      if (error instanceof Error) {
        if (error.message.includes('foreign key constraint')) {
          errorMessage = 'Cannot delete agent because it is referenced by other entities. Please remove all dependencies first.';
        } else {
          errorMessage = `Failed to delete agent: ${error.message}`;
        }
      }
      
      showNotification('Error', errorMessage, 'error');
    } finally {
      setIsLoading(false);
      closeConfirmation();
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

      {/* Notification Modal */}
      <NotificationModal
        isOpen={notification.isOpen}
        onClose={closeNotification}
        title={notification.title}
        message={notification.message}
        type={notification.type}
      />

      {/* Confirmation Modal */}
      <ConfirmationModal
        isOpen={confirmation.isOpen}
        onClose={closeConfirmation}
        onConfirm={handleConfirmDelete}
        title={confirmation.title}
        message={confirmation.message}
        confirmText="Delete"
        cancelText="Cancel"
        type="danger"
        isLoading={isLoading}
      />
    </div>
  );
}

export default AgentDetailsTab;
