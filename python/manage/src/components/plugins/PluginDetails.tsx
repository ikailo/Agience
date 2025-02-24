import { useState } from 'react';
import { Button } from '../common/Button';
import { PluginForm } from './PluginForm';


type ProviderType = 'Collection' | 'Prompts' | 'Semantic Kernel Plugin';

interface Plugin {
  id: string;
  name: string;
  description: string;
  provider: ProviderType;
}

export const PluginDetails = () => {
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingPlugin, setEditingPlugin] = useState<Plugin | null>(null);
  const [plugins, setPlugins] = useState<Plugin[]>([
    {
      id: '1',
      name: 'Weather Plugin',
      description: 'Provides real-time weather information and forecasts',
      provider: 'Semantic Kernel Plugin',
    },
    {
      id: '2',
      name: 'Task Templates',
      description: 'Collection of reusable task templates',
      provider: 'Collection',
    },
  ]);

  const handleEdit = (plugin: Plugin) => {
    setEditingPlugin(plugin);
    setIsFormOpen(true);
  };

  const handleDelete = (id: string) => {
    setPlugins(plugins.filter(plugin => plugin.id !== id));
  };

  const handleSave = (plugin: Plugin) => {
    if (editingPlugin) {
      setPlugins(plugins.map(p => p.id === plugin.id ? plugin : p));
    } else {
      setPlugins([...plugins, { ...plugin, id: Date.now().toString() }]);
    }
    setIsFormOpen(false);
    setEditingPlugin(null);
  };

  const handleAdd = () => {
    setEditingPlugin(null);
    setIsFormOpen(true);
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Plugins
        </h2>
        <Button
          variant="primary"
          onClick={handleAdd}
        >
          Add Plugin
        </Button>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full">
          <thead className="bg-gray-900 dark:bg-gray-800">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Name</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Description</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Provider</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-gray-800 dark:bg-gray-900 divide-y divide-gray-700">
            {plugins.map(plugin => (
              <tr key={plugin.id} className="hover:bg-gray-700 transition-colors">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                  {plugin.name}
                </td>
                <td className="px-6 py-4 text-sm text-gray-300">
                  {plugin.description}
                </td>
                <td className="px-6 py-4 text-sm text-gray-300">
                  {plugin.provider}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                  <button
                    onClick={() => handleEdit(plugin)}
                    className="text-blue-400 hover:text-blue-300 mr-4"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(plugin.id)}
                    className="text-red-400 hover:text-red-300"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {isFormOpen && (
        <PluginForm
          plugin={editingPlugin}
          onSave={handleSave}
          onCancel={() => {
            setIsFormOpen(false);
            setEditingPlugin(null);
          }}
        />
      )}
    </div>
  );
}; 