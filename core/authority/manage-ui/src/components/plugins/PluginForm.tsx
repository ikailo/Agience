import React, { useState, useEffect } from 'react';
import { PluginFormData } from '../../types/Plugin';

interface PluginFormProps {
  onSubmit: (formData: PluginFormData) => Promise<void>;
  onCancel: () => void;
  isLoading: boolean;
  initialData?: PluginFormData;
}

/**
 * Form component for creating or editing plugins
 */
const PluginForm: React.FC<PluginFormProps> = ({
  onSubmit,
  onCancel,
  isLoading,
  initialData
}) => {
  const [formData, setFormData] = useState<PluginFormData>({
    name: initialData?.name || '',
    description: initialData?.description || '',
    provider: initialData?.provider || 'Collection'
  });

  const [errors, setErrors] = useState<{
    name?: string;
    description?: string;
    provider?: string;
  }>({});

  // Update form data when initialData changes
  useEffect(() => {
    if (initialData) {
      setFormData({
        name: initialData.name,
        description: initialData.description,
        provider: initialData.provider
      });
    }
  }, [initialData]);

  /**
   * Handles form input changes
   */
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Clear error when field is edited
    if (errors[name as keyof typeof errors]) {
      setErrors(prev => ({ ...prev, [name]: undefined }));
    }
  };

  /**
   * Validates form data
   */
  const validate = (): boolean => {
    const newErrors: {
      name?: string;
      description?: string;
      provider?: string;
    } = {};
    
    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    }
    
    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    }
    
    if (!formData.provider) {
      newErrors.provider = 'Provider is required';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  /**
   * Handles form submission
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validate()) {
      return;
    }
    
    await onSubmit(formData);
  };

  const isEditing = !!initialData;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-gray-900 rounded-lg p-6 w-full max-w-2xl">
        <h3 className="text-lg font-medium text-white mb-4">
          {isEditing ? 'Edit Plugin' : 'Add New Plugin'}
        </h3>

        {Object.values(errors).some(error => error) && (
          <div className="mb-4 p-3 bg-red-900 border border-red-700 text-red-300 rounded">
            Please fix the errors below.
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-300">
              Name
              {errors.name && <span className="ml-2 text-red-400 text-xs">{errors.name}</span>}
            </label>
            <input
              type="text"
              id="name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              className="mt-1 block w-full rounded-md bg-gray-800 border-gray-700 text-white focus:border-indigo-500 focus:ring-indigo-500"
              disabled={isLoading}
              required
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-300">
              Description
              {errors.description && <span className="ml-2 text-red-400 text-xs">{errors.description}</span>}
            </label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows={3}
              className="mt-1 block w-full rounded-md bg-gray-800 border-gray-700 text-white focus:border-indigo-500 focus:ring-indigo-500"
              disabled={isLoading}
              required
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-300">
              Provider
              {errors.provider && <span className="ml-2 text-red-400 text-xs">{errors.provider}</span>}
            </label>
            <select
              id="provider"
              name="provider"
              value={formData.provider}
              onChange={handleChange}
              className="mt-1 block w-full rounded-md bg-gray-800 border-gray-700 text-white focus:border-indigo-500 focus:ring-indigo-500"
              disabled={isLoading}
              required
            >
              <option value="Collection">Collection</option>
              <option value="Prompts">Prompts</option>
              <option value="Semantic Kernel Plugin">Semantic Kernel Plugin</option>
            </select>
          </div>
          
          <div className="flex justify-end space-x-3 mt-6">
            <button
              type="button"
              onClick={onCancel}
              className="px-4 py-2 bg-gray-700 text-white rounded-md hover:bg-gray-600 transition-colors"
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors"
              disabled={isLoading}
            >
              {isLoading ? (
                <span className="flex items-center">
                  <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  {isEditing ? 'Saving...' : 'Creating...'}
                </span>
              ) : (
                isEditing ? 'Save Changes' : 'Create Plugin'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PluginForm; 