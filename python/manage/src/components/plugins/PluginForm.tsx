import { useState } from 'react';
import { Button } from '../common/Button';

type ProviderType = 'Collection' | 'Prompts' | 'Semantic Kernel Plugin';

interface Plugin {
  id: string;
  name: string;
  description: string;
  provider: ProviderType;
}

interface PluginFormProps {
  plugin: Plugin | null;
  onSave: (plugin: Plugin) => void;
  onCancel: () => void;
}

const PROVIDER_OPTIONS: ProviderType[] = ['Collection', 'Prompts', 'Semantic Kernel Plugin'];

export const PluginForm = ({ plugin, onSave, onCancel }: PluginFormProps) => {
  const [formData, setFormData] = useState<Omit<Plugin, 'id'>>({
    name: plugin?.name || '',
    description: plugin?.description || '',
    provider: plugin?.provider || 'Collection',
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave({
      id: plugin?.id || '',
      ...formData,
    });
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center">
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-2xl">
        <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
          {plugin ? 'Edit Plugin' : 'Add Plugin'}
        </h3>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Name
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={e => setFormData(prev => ({ ...prev, name: e.target.value }))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Description
            </label>
            <input
              type="text"
              value={formData.description}
              onChange={e => setFormData(prev => ({ ...prev, description: e.target.value }))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Provider
            </label>
            <select
              value={formData.provider}
              onChange={e => setFormData(prev => ({ ...prev, provider: e.target.value as ProviderType }))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            >
              {PROVIDER_OPTIONS.map(option => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>

          <div className="flex justify-end space-x-3 mt-6">
            <Button variant="secondary" onClick={onCancel}>
              Cancel
            </Button>
            <Button variant="primary" type="submit">
              {plugin ? 'Save Changes' : 'Add Plugin'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}; 