import { useState, useRef } from 'react';
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
  image?: File;
  imagePreview?: string;
}

export const AgentDetailsForm: React.FC<AgentDetailsFormProps> = ({
  onSave,
  onDelete,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [formData, setFormData] = useState<AgentFormData>({
    name: '',
    description: '',
    persona: '',
    host: '',
    executiveFunction: '',
    enabled: false,
  });

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

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData);
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