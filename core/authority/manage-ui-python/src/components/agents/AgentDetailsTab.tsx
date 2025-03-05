import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { agentService } from '../../services/api/agentService';
import { Agent, AgentFormData } from '../../types/Agent';

/**
 * AgentDetailsTab component that displays a list of agents and a form to edit agent details
 */
function AgentDetailsTab() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [agents, setAgents] = useState<Agent[]>([]);
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

  // Fetch all agents on component mount
  useEffect(() => {
    fetchAgents();
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
      setFormData(prev => ({ ...prev, [name]: value }));
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
    <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
      {/* Agent List Section */}
      <div className="md:col-span-1 bg-gray-900 p-4 rounded-lg">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-semibold text-white">Agent List</h2>
          <button
            onClick={handleCreateAgent}
            className="px-3 py-1 bg-indigo-600 text-white rounded hover:bg-indigo-700 transition-colors"
          >
            New Agent
          </button>
        </div>
        
        {isLoading && agents.length === 0 ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
          </div>
        ) : (
          <div className="space-y-2 max-h-[calc(100vh-250px)] overflow-y-auto">
            {agents.length === 0 ? (
              <p className="text-gray-400 text-center py-4">No agents found</p>
            ) : (
              agents.map(agent => (
                <div
                  key={agent.id}
                  onClick={() => handleSelectAgent(agent.id)}
                  className={`p-3 rounded cursor-pointer transition-colors ${
                    selectedAgent?.id === agent.id
                      ? 'bg-indigo-700 text-white'
                      : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                  }`}
                >
                  <div className="font-medium">{agent.name}</div>
                  <div className="text-sm text-gray-400 truncate">
                    {agent.description}
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>

      {/* Agent Details Form Section */}
      <div className="md:col-span-3">
        <div className="bg-gray-800 p-6 rounded-lg">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Name Field */}
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-300 mb-1">
                Name
              </label>
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                className="w-full bg-gray-700 border border-gray-600 rounded-md py-2 px-3 text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            {/* Description Field */}
            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-300 mb-1">
                Description
              </label>
              <input
                type="text"
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                className="w-full bg-gray-700 border border-gray-600 rounded-md py-2 px-3 text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            {/* Persona Field */}
            <div>
              <label htmlFor="persona" className="block text-sm font-medium text-gray-300 mb-1">
                Persona
              </label>
              <textarea
                id="persona"
                name="persona"
                value={formData.persona || ''}
                onChange={handleInputChange}
                rows={4}
                className="w-full bg-gray-700 border border-gray-600 rounded-md py-2 px-3 text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            {/* Enabled Checkbox */}
            <div className="flex items-center">
              <input
                type="checkbox"
                id="is_enabled"
                name="is_enabled"
                checked={formData.is_enabled}
                onChange={handleInputChange}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-600 rounded"
              />
              <label htmlFor="is_enabled" className="ml-2 block text-sm text-gray-300">
                Enabled
              </label>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="mt-6 flex space-x-4">
            <button
              onClick={handleSave}
              disabled={isLoading}
              className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 transition-colors disabled:opacity-50"
            >
              {isLoading ? 'Saving...' : 'Save'}
            </button>
            {selectedAgent && (
              <button
                onClick={handleDelete}
                disabled={isLoading}
                className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition-colors disabled:opacity-50"
              >
                {isLoading ? 'Deleting...' : 'Delete'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default AgentDetailsTab;
