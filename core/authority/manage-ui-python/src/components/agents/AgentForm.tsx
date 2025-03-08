import React from 'react';
import { Agent, AgentFormData } from '../../types/Agent';
import { Host } from '../../types/Host';

interface AgentFormProps {
  formData: AgentFormData;
  selectedAgent: Agent | null;
  isLoading: boolean;
  hosts: Host[];
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => void;
  onSave: () => void;
  onDelete: () => void;
  isTempAgent?: boolean;
}

/**
 * AgentForm component for creating and editing agent details
 */
const AgentForm: React.FC<AgentFormProps> = ({
  formData,
  selectedAgent,
  isLoading,
  hosts,
  onChange,
  onSave,
  onDelete,
  isTempAgent = false
}) => {
  return (
    <div className="bg-white dark:bg-gray-800 p-4 sm:p-6 rounded-lg shadow-lg">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          {isTempAgent ? 'Create New Agent' : selectedAgent ? 'Edit Agent' : 'Agent Details'}
        </h2>
        {isTempAgent && (
          <span className="px-2 py-1 bg-indigo-100 text-indigo-800 dark:bg-indigo-800 dark:text-indigo-200 text-xs rounded-full">
            Unsaved
          </span>
        )}
      </div>
      
      {!selectedAgent && !isTempAgent ? (
        <div className="text-center py-8">
          <p className="text-gray-600 dark:text-gray-400">
            Select an agent from the list or create a new one to get started.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
          {/* Name Field */}
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Name
            </label>
            <input
              type="text"
              id="name"
              name="name"
              value={formData.name}
              onChange={onChange}
              className="w-full px-3 py-2 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              disabled={isLoading}
              placeholder="Enter agent name"
            />
          </div>

          {/* Host Field */}
          <div>
            <label htmlFor="hostId" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Host
            </label>
            <select
              id="hostId"
              name="hostId"
              value={formData.hostId || ''}
              onChange={onChange}
              className="w-full px-3 py-2 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              disabled={isLoading}
            >
              <option value="">Select a host</option>
              {hosts.map(host => (
                <option key={host.id} value={host.id}>
                  {host.name}
                </option>
              ))}
            </select>
          </div>

          {/* Description Field - Full Width */}
          <div className="lg:col-span-2">
            <label htmlFor="description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Description
            </label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={onChange}
              rows={3}
              className="w-full px-3 py-2 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              disabled={isLoading}
              placeholder="Enter agent description"
            />
          </div>

          {/* Enabled Checkbox */}
          <div className="lg:col-span-2">
            <div className="flex items-center">
              <input
                type="checkbox"
                id="is_enabled"
                name="is_enabled"
                checked={formData.is_enabled}
                onChange={(e) => {
                  // Create a custom event that directly modifies the formData
                  const customEvent = {
                    target: {
                      name: 'is_enabled',
                      type: 'checkbox',
                      checked: e.target.checked
                    }
                  } as React.ChangeEvent<HTMLInputElement>;
                  
                  onChange(customEvent);
                }}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                disabled={isLoading}
              />
              <label htmlFor="is_enabled" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                Enabled
              </label>
            </div>
          </div>

          {/* Buttons - Full Width */}
          <div className="lg:col-span-2 flex flex-wrap justify-end gap-3 mt-4">
            {selectedAgent && !isTempAgent && (
              <button
                type="button"
                onClick={onDelete}
                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={isLoading}
              >
                {isLoading ? 'Deleting...' : 'Delete Agent'}
              </button>
            )}
            <button
              type="button"
              onClick={onSave}
              className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              disabled={isLoading}
            >
              {isLoading ? 'Saving...' : isTempAgent ? 'Create Agent' : 'Update Agent'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default AgentForm; 