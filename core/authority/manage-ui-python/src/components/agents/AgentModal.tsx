import { useState, useEffect } from 'react';
import { AgentFormData } from '../../types/Agent';
import { hostService } from '../../services/api/hostService';
import { Host } from '../../types/Host';

interface AgentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (agentData: AgentFormData) => Promise<void>;
  initialData?: AgentFormData;
  isEditing?: boolean;
}

// Mock executive functions for now
const mockExecutiveFunctions = [
  { id: 'ef1', name: 'Basic Conversation' },
  { id: 'ef2', name: 'Task Management' },
  { id: 'ef3', name: 'Data Analysis' },
];

export const AgentModal: React.FC<AgentModalProps> = ({
  isOpen,
  onClose,
  onSave,
  initialData,
  isEditing = false,
}) => {
  // Form state
  const [formData, setFormData] = useState<AgentFormData>({
    name: '',
    description: '',
    persona: '',
    hostId: '',
    executiveFunctionId: '',
    is_enabled: false,
  });
  
  // Form validation state
  const [errors, setErrors] = useState<Record<string, string>>({});
  
  // Loading states
  const [isSaving, setIsSaving] = useState(false);
  const [isLoadingHosts, setIsLoadingHosts] = useState(false);
  
  // Hosts data
  const [hosts, setHosts] = useState<Host[]>([]);

  // Update form data when initialData changes
  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    } else {
      // Reset to default values
      setFormData({
        name: '',
        description: '',
        persona: '',
        hostId: '',
        executiveFunctionId: '',
        is_enabled: false,
      });
    }
  }, [initialData]);

  // Fetch hosts when component mounts
  useEffect(() => {
    fetchHosts();
  }, []);

  // Function to fetch all hosts
  const fetchHosts = async () => {
    try {
      setIsLoadingHosts(true);
      const hostsData = await hostService.getAllHosts();
      setHosts(hostsData);
    } catch (err) {
      console.error('Error fetching hosts:', err);
    } finally {
      setIsLoadingHosts(false);
    }
  };

  // Handle input changes
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target as HTMLInputElement;
    
    if (type === 'checkbox') {
      const { checked } = e.target as HTMLInputElement;
      setFormData((prev) => ({
        ...prev,
        [name]: checked,
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        [name]: value,
      }));
    }
    
    // Clear error for this field when user types
    if (errors[name]) {
      setErrors((prev) => ({
        ...prev,
        [name]: '',
      }));
    }
  };

  // Validate form data
  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.name.trim()) {
      newErrors.name = 'Agent name is required';
    }
    
    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }
    
    try {
      setIsSaving(true);
      await onSave(formData);
      onClose();
    } catch (error) {
      console.error('Error saving agent:', error);
      // You could add error handling here, e.g., display an error message
    } finally {
      setIsSaving(false);
    }
  };

  // If modal is not open, don't render anything
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            {isEditing ? 'Edit Agent' : 'Add New Agent'}
          </h2>
          
          <form onSubmit={handleSubmit}>
            <div className="space-y-4">
              {/* Agent Name */}
              <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
                    errors.name ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'
                  }`}
                  placeholder="Enter agent name"
                />
                {errors.name && (
                  <p className="mt-1 text-sm text-red-500">{errors.name}</p>
                )}
              </div>
              
              {/* Agent Description */}
              <div>
                <label htmlFor="description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Description <span className="text-red-500">*</span>
                </label>
                <textarea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleChange}
                  rows={3}
                  className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
                    errors.description ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'
                  }`}
                  placeholder="Enter agent description"
                />
                {errors.description && (
                  <p className="mt-1 text-sm text-red-500">{errors.description}</p>
                )}
              </div>
              
              {/* Agent Persona */}
              <div>
                <label htmlFor="persona" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Persona
                </label>
                <textarea
                  id="persona"
                  name="persona"
                  value={formData.persona || ''}
                  onChange={handleChange}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                  placeholder="Enter agent persona (optional)"
                />
              </div>
              
              {/* Host Selection */}
              <div>
                <label htmlFor="host_id" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Host
                </label>
                <select
                  id="host_id"
                  name="host_id"
                  value={formData.hostId || ''}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                >
                  <option value="">Select a host</option>
                  {isLoadingHosts ? (
                    <option disabled>Loading hosts...</option>
                  ) : (
                    hosts.map(host => (
                      <option key={host.id} value={host.id}>
                        {host.name}
                      </option>
                    ))
                  )}
                </select>
              </div>
              
              {/* Executive Function */}
              <div>
                <label htmlFor="executive_function_id" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Executive Function
                </label>
                <select
                  id="executive_function_id"
                  name="executive_function_id"
                  value={formData.executiveFunctionId || ''}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                >
                  <option value="">Select a function</option>
                  {mockExecutiveFunctions.map(func => (
                    <option key={func.id} value={func.id}>
                      {func.name}
                    </option>
                  ))}
                </select>
              </div>
              
              {/* Enabled Checkbox */}
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="is_enabled"
                  name="is_enabled"
                  checked={formData.is_enabled}
                  onChange={handleChange}
                  className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600"
                />
                <label htmlFor="is_enabled" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                  Enabled
                </label>
              </div>
            </div>
            
            {/* Action Buttons */}
            <div className="mt-6 flex justify-end space-x-3">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white dark:hover:bg-gray-600"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSaving}
                className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 dark:bg-blue-500 dark:hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSaving ? (
                  <span className="flex items-center">
                    <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Saving...
                  </span>
                ) : (
                  'Save Agent'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}; 