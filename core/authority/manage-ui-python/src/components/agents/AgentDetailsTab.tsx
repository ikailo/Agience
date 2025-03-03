import { useState, useEffect } from 'react';
import { AgentDetailsForm, AgentFormData } from './AgentDetailsForm';
import { AgentList } from './AgentList';
import { Agent } from '../../types/Agent';
import { agentService } from '../../services/api/agentService';

interface AgentDetailsTabProps {
  selectedAgent: Agent | null;
  onSelectAgent: (id: string) => void;
  onSave: (data: AgentFormData) => void;
  onDelete: () => void;
}

export const AgentDetailsTab: React.FC<AgentDetailsTabProps> = ({
  selectedAgent,
  onSelectAgent,
  onSave,
  onDelete,
}) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [currentAgent, setCurrentAgent] = useState<Agent | null>(null);

  // Update current agent when selectedAgent changes
  useEffect(() => {
    setCurrentAgent(selectedAgent);
  }, [selectedAgent]);

  // Convert API agent data to form data format if an agent is selected
  const getInitialFormData = (): AgentFormData | undefined => {
    if (!currentAgent) return undefined;
    
    return {
      name: currentAgent.name,
      description: currentAgent.description,
      persona: currentAgent.persona,
      host_id: currentAgent.host_id,
      executive_function_id: currentAgent.executive_function_id,
      is_enabled: currentAgent.is_enabled,
      imagePreview: currentAgent.imageUrl,
    };
  };

  // Handle form submission
  const handleSaveAgent = async (formData: AgentFormData) => {
    try {
      setIsLoading(true);
      setError(null);
      
      if (currentAgent) {
        // Update existing agent
        await agentService.updateAgent(currentAgent.id, formData);
        setSuccessMessage('Agent updated successfully!');
      } else {
        // Create new agent
        await agentService.createAgent(formData);
        setSuccessMessage('Agent created successfully!');
      }
      
      // Call the parent's onSave handler
      onSave(formData);
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
      
    } catch (err) {
      console.error('Error saving agent:', err);
      setError(`Failed to ${currentAgent ? 'update' : 'create'} agent. Please try again.`);
    } finally {
      setIsLoading(false);
    }
  };

  // Handle deleting the current agent
  const handleDeleteAgent = async () => {
    if (!currentAgent) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      await agentService.deleteAgent(currentAgent.id);
      
      setSuccessMessage('Agent deleted successfully!');
      setCurrentAgent(null);
      
      // Call the parent's onDelete handler
      onDelete();
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
      
    } catch (err) {
      console.error('Error deleting agent:', err);
      setError('Failed to delete agent. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  // Handle successful deletion from the list
  const handleDeleteSuccess = () => {
    // If the deleted agent was the currently selected one, clear it
    if (currentAgent && !agentService.getAgentById(currentAgent.id).then) {
      setCurrentAgent(null);
      onDelete();
    }
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <div>
        {/* Success message */}
        {successMessage && (
          <div className="mb-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-4">
            <p className="text-green-600 dark:text-green-400">{successMessage}</p>
          </div>
        )}

        {/* Error message */}
        {error && (
          <div className="mb-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4">
            <p className="text-red-600 dark:text-red-400">{error}</p>
          </div>
        )}

        <AgentDetailsForm
          initialData={getInitialFormData()}
          onSave={handleSaveAgent}
          onDelete={handleDeleteAgent}
          isEditing={!!currentAgent}
        />
      </div>
      <div className="mt-8 lg:mt-0">
        <AgentList 
          onSelectAgent={onSelectAgent} 
          onDeleteSuccess={handleDeleteSuccess}
        />
      </div>
    </div>
  );
}; 