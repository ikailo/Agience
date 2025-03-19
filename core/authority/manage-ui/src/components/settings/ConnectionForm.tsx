import React, { useState, useEffect } from 'react';
import { ConnectionFormData } from '../../types/Connection';
import { Authorizer } from '../../types/Authorizer';

interface ConnectionFormProps {
  onSubmit: (formData: ConnectionFormData) => Promise<void>;
  onCancel: () => void;
  isLoading: boolean;
  initialData?: ConnectionFormData;
  authorizers: Authorizer[];
}

/**
 * Form component for creating or editing connections
 */
const ConnectionForm: React.FC<ConnectionFormProps> = ({
  onSubmit,
  onCancel,
  isLoading,
  initialData,
  authorizers
}) => {
  const [formData, setFormData] = useState<ConnectionFormData>({
    name: initialData?.name || '',
    description: initialData?.description || '',
    authorizer_id: initialData?.authorizer_id || ''
  });

  const [errors, setErrors] = useState<{
    name?: string;
    description?: string;
    authorizer_id?: string;
  }>({});

  // Update form data when initialData changes
  useEffect(() => {
    if (initialData) {
      setFormData({
        name: initialData.name,
        description: initialData.description,
        authorizer_id: initialData.authorizer_id
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
      authorizer_id?: string;
    } = {};
    
    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    }
    
    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    }
    
    if (!formData.authorizer_id) {
      newErrors.authorizer_id = 'Authorizer is required';
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
  const selectedAuthorizer = authorizers.find(a => a.id === formData.authorizer_id);

  return (
    <div className="bg-gray-800 rounded-lg p-6">
      <h3 className="text-lg font-medium text-white mb-4">
        {isEditing ? 'Edit Connection' : 'Add New Connection'}
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
            className="mt-1 block w-full rounded-md bg-gray-700 border-gray-600 text-white focus:border-indigo-500 focus:ring-indigo-500"
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
            className="mt-1 block w-full rounded-md bg-gray-700 border-gray-600 text-white focus:border-indigo-500 focus:ring-indigo-500"
            disabled={isLoading}
            required
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-300">
            Authorizer
            {errors.authorizer_id && <span className="ml-2 text-red-400 text-xs">{errors.authorizer_id}</span>}
          </label>
          <select
            id="authorizer_id"
            name="authorizer_id"
            value={formData.authorizer_id}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md bg-gray-700 border-gray-600 text-white focus:border-indigo-500 focus:ring-indigo-500"
            disabled={isLoading}
            required
          >
            <option value="">Select an authorizer</option>
            {authorizers.map(authorizer => (
              <option key={authorizer.id} value={authorizer.id}>
                {authorizer.name} ({authorizer.type})
              </option>
            ))}
          </select>
        </div>

        {selectedAuthorizer && (
          <div className="p-3 bg-gray-700 rounded-md">
            <h4 className="text-sm font-medium text-gray-300 mb-2">Authorizer Details</h4>
            <div className="grid grid-cols-2 gap-2 text-sm">
              <div>
                <span className="text-gray-400">Type:</span>
                <span className="ml-2 text-white">{selectedAuthorizer.type}</span>
              </div>
              <div>
                <span className="text-gray-400">ID:</span>
                <span className="ml-2 text-white font-mono text-xs">{selectedAuthorizer.id}</span>
              </div>
            </div>
          </div>
        )}
        
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
              isEditing ? 'Save Changes' : 'Create Connection'
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ConnectionForm; 