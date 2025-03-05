import { useState } from 'react';
import { Button } from '../common/Button';
import { pluginService } from '../../services/api/pluginService';
import { Plugin, PluginFormData, ProviderType } from '../../types/Plugin';

interface PluginFormProps {
  plugin: Plugin | null;
  onSave: (plugin: Plugin) => void;
  onCancel: () => void;
}

const PROVIDER_OPTIONS: ProviderType[] = ['Collection', 'Prompts', 'Semantic Kernel Plugin'];

export const PluginForm = ({ plugin, onSave, onCancel }: PluginFormProps) => {
  const [formData, setFormData] = useState<PluginFormData>({
    name: plugin?.name || '',
    description: plugin?.description || '',
    provider: plugin?.provider || 'Collection',
  });
  
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
      setError('Plugin name is required');
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      let savedPlugin: Plugin;
      
      if (plugin?.id) {
        // Update existing plugin
        savedPlugin = await pluginService.updatePlugin(plugin.id, formData);
      } else {
        // Create new plugin
        savedPlugin = await pluginService.createPlugin(formData);
      }
      
      onSave(savedPlugin);
    } catch (err) {
      console.error('Error saving plugin:', err);
      setError('Failed to save plugin. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-2xl">
        <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
          {plugin ? 'Edit Plugin' : 'Add Plugin'}
        </h3>

        {error && (
          <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

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
              required
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
            <Button variant="secondary" onClick={onCancel} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button variant="primary" type="submit" disabled={isSubmitting}>
              {isSubmitting ? (
                <span className="flex items-center">
                  <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Saving...
                </span>
              ) : (
                plugin ? 'Save Changes' : 'Add Plugin'
              )}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}; 