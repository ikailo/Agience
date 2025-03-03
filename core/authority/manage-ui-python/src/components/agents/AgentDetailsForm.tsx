import { useState, useEffect, useRef } from 'react';
import { Button } from '../common/Button';
import { hostService } from '../../services/api/hostService';
import { Host } from '../../types/Host';

interface AgentDetailsFormProps {
  onSave: (data: AgentFormData) => void;
  onDelete?: () => void;
  initialData?: AgentFormData;
  isEditing?: boolean;
}

export interface AgentFormData {
  name: string;
  description: string;
  persona: string | null;
  hostId: string | null;
  executiveFunctionId: string | null;
  is_enabled: boolean;
  image?: File;
  imagePreview?: string;
}

// Mock executive functions for now
const mockExecutiveFunctions = [
  { id: 'ef1', name: 'Basic Conversation' },
  { id: 'ef2', name: 'Task Management' },
  { id: 'ef3', name: 'Data Analysis' },
];

export const AgentDetailsForm: React.FC<AgentDetailsFormProps> = ({
  onSave,
  onDelete,
  initialData,
  isEditing = false,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [formData, setFormData] = useState<AgentFormData>({
    name: '',
    description: '',
    persona: '',
    hostId: '',
    executiveFunctionId: '',
    is_enabled: false,
  });
  
  const [hosts, setHosts] = useState<Host[]>([]);
  const [isLoadingHosts, setIsLoadingHosts] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Fetch hosts when component mounts
  useEffect(() => {
    fetchHosts();
  }, []);

  // Initialize form with initial data if provided
  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

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

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setFormData(prev => ({
        ...prev,
        image: file,
        imagePreview: URL.createObjectURL(file),
      }));
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    
    // Handle checkbox separately
    if (type === 'checkbox') {
      const checked = (e.target as HTMLInputElement).checked;
      setFormData(prev => ({
        ...prev,
        [name]: checked,
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: value,
      }));
    }
    
    // Clear error for this field when user types
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: '',
      }));
    }
  };

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

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }
    
    onSave(formData);
    window.location.reload();
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Image Upload */}
      <div>
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Agent Avatar
        </label>
        <div className="mt-1 flex items-center space-x-4">
          <div 
            className="w-24 h-24 rounded-full overflow-hidden bg-gray-100 dark:bg-gray-700 flex items-center justify-center"
          >
            {formData.imagePreview ? (
              <img
                src={formData.imagePreview}
                alt="Preview"
                className="w-full h-full object-cover"
              />
            ) : (
              <svg className="h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            )}
          </div>
          <Button
            type="button"
            variant="secondary"
            onClick={() => fileInputRef.current?.click()}
          >
            Change Image
          </Button>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleImageChange}
            accept="image/*"
            className="hidden"
          />
        </div>
      </div>

      {/* Name Input */}
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Name <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          id="name"
          name="name"
          value={formData.name}
          onChange={handleChange}
          className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
            errors.name ? 'border-red-500' : ''
          }`}
        />
        {errors.name && (
          <p className="mt-1 text-sm text-red-500">{errors.name}</p>
        )}
      </div>

      {/* Description Input */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Description <span className="text-red-500">*</span>
        </label>
        <textarea
          id="description"
          name="description"
          rows={3}
          value={formData.description}
          onChange={handleChange}
          className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
            errors.description ? 'border-red-500' : ''
          }`}
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-500">{errors.description}</p>
        )}
      </div>

      {/* Persona Input */}
      <div>
        <label htmlFor="persona" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Persona
        </label>
        <textarea
          id="persona"
          name="persona"
          rows={3}
          value={formData.persona || ''}
          onChange={handleChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        />
      </div>

      {/* Host Dropdown */}
      <div>
        <label htmlFor="hostId" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Host
        </label>
        <select
          id="hostId"
          name="hostId"
          value={formData.hostId || ''}
          onChange={handleChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
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

      {/* Executive Function Dropdown */}
      <div>
        <label htmlFor="executiveFunctionId" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Executive Function
        </label>
        <select
          id="executiveFunctionId"
          name="executiveFunctionId"
          value={formData.executiveFunctionId || ''}
          onChange={handleChange}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        >
          <option value="">Select a function</option>
          {mockExecutiveFunctions.map(func => (
            <option key={func.id} value={func.id}>
              {func.name}
            </option>
          ))}
        </select>
      </div>

      {/* Action Buttons */}
      <div className="flex space-x-4">
        <Button type="submit" variant="primary">
          {isEditing ? 'Update' : 'Create'} Agent
        </Button>
        {isEditing && onDelete && (
          <Button type="button" variant="danger" onClick={onDelete}>
            Delete
          </Button>
        )}
      </div>
    </form>
  );
}; 