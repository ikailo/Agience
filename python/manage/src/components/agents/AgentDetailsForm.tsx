import { useState } from 'react';
import { Button } from '../common/Button';

interface AgentDetailsFormProps {
  onSave: (data: AgentFormData) => void;
  onDelete: () => void;
}

export interface AgentFormData {
  name: string;
  description: string;
  persona: string;
  host: string;
  executiveFunction: string;
  enabled: boolean;
}

export const AgentDetailsForm: React.FC<AgentDetailsFormProps> = ({
  onSave,
  onDelete,
}) => {
  const [formData, setFormData] = useState<AgentFormData>({
    name: '',
    description: '',
    persona: '',
    host: '',
    executiveFunction: '',
    enabled: false,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6 max-w-2xl">
      {/* Name Input */}
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Name
        </label>
        <input
          type="text"
          id="name"
          value={formData.name}
          onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        />
      </div>

      {/* Description Input */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Description
        </label>
        <textarea
          id="description"
          rows={3}
          value={formData.description}
          onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        />
      </div>

      {/* Persona Input */}
      <div>
        <label htmlFor="persona" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Persona
        </label>
        <textarea
          id="persona"
          rows={3}
          value={formData.persona}
          onChange={(e) => setFormData(prev => ({ ...prev, persona: e.target.value }))}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        />
      </div>

      {/* Host Dropdown */}
      <div>
        <label htmlFor="host" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Host
        </label>
        <select
          id="host"
          value={formData.host}
          onChange={(e) => setFormData(prev => ({ ...prev, host: e.target.value }))}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        >
          <option value="">Select a host</option>
          {/* Add host options here */}
        </select>
      </div>

      {/* Executive Function Dropdown */}
      <div>
        <label htmlFor="executiveFunction" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
          Executive Function
        </label>
        <select
          id="executiveFunction"
          value={formData.executiveFunction}
          onChange={(e) => setFormData(prev => ({ ...prev, executiveFunction: e.target.value }))}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        >
          <option value="">Select a function</option>
          {/* Add function options here */}
        </select>
      </div>

      {/* Enabled Checkbox */}
      <div className="flex items-center">
        <input
          type="checkbox"
          id="enabled"
          checked={formData.enabled}
          onChange={(e) => setFormData(prev => ({ ...prev, enabled: e.target.checked }))}
          className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600"
        />
        <label htmlFor="enabled" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
          Enabled
        </label>
      </div>

      {/* Action Buttons */}
      <div className="flex space-x-4">
        <Button type="submit" variant="primary">
          Save
        </Button>
        <Button type="button" variant="danger" onClick={onDelete}>
          Delete
        </Button>
      </div>
    </form>
  );
}; 