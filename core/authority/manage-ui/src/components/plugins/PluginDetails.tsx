import { useState, useEffect } from 'react';
import Button  from '../common/Button';
import  PluginForm  from './PluginForm';
import { PluginTable } from './PluginTable';
import { pluginService } from '../../services/api/pluginService';
import { Plugin } from '../../types/Plugin';

export const PluginDetails: React.FC = () => {
  const [plugins, setPlugins] = useState<Plugin[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [selectedPlugin, setSelectedPlugin] = useState<Plugin | null>(null);
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
  const [pluginToDelete, setPluginToDelete] = useState<Plugin | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Fetch plugins when component mounts
  useEffect(() => {
    fetchPlugins();
  }, []);

  // Function to fetch all plugins
  const fetchPlugins = async () => {
    try {
      setIsLoading(true);
      const pluginsData = await pluginService.getAllPlugins();
      setPlugins(pluginsData);
      setError(null);
    } catch (err) {
      console.error('Error fetching plugins:', err);
      setError('Failed to load plugins. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  // Function to handle adding/editing a plugin
  const handleSavePlugin = async () => {
    try {
      // Show success message
      setSuccessMessage(selectedPlugin ? 'Plugin updated successfully!' : 'Plugin created successfully!');
      
      // Refresh the plugins list
      fetchPlugins();
      
      // Close the form
      setIsFormOpen(false);
      setSelectedPlugin(null);
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
    } catch (err) {
      console.error('Error saving plugin:', err);
      setError('Failed to save plugin. Please try again.');
    }
  };

  // Function to handle deleting a plugin
  const handleDeletePlugin = async () => {
    if (!pluginToDelete) return;
    
    try {
      await pluginService.deletePlugin(pluginToDelete.id);
      
      // Show success message
      setSuccessMessage('Plugin deleted successfully!');
      
      // Refresh the plugins list
      fetchPlugins();
      
      // Close the delete confirmation
      setIsDeleteConfirmOpen(false);
      setPluginToDelete(null);
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
    } catch (err) {
      console.error('Error deleting plugin:', err);
      setError('Failed to delete plugin. Please try again.');
    }
  };

  // Function to open the form for adding a new plugin
  const handleAddPlugin = () => {
    setSelectedPlugin(null);
    setIsFormOpen(true);
  };

  // Function to open the form for editing a plugin
  const handleEditPlugin = (plugin: Plugin) => {
    setSelectedPlugin(plugin);
    setIsFormOpen(true);
  };

  // Function to open delete confirmation
  const handleDeleteClick = (plugin: Plugin) => {
    setPluginToDelete(plugin);
    setIsDeleteConfirmOpen(true);
  };

  // Function to handle selecting a plugin (for future implementation)
  const handleSelectPlugin = (plugin: Plugin) => {
    console.log('Selected plugin:', plugin);
    // Implement navigation or detail view here
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-medium text-gray-900 dark:text-white">
          Plugins
        </h2>
        <Button variant="primary" onClick={handleAddPlugin}>
          Add Plugin
        </Button>
      </div>

      {/* Success message */}
      {successMessage && (
        <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-4">
          <p className="text-green-600 dark:text-green-400">{successMessage}</p>
        </div>
      )}

      {/* Error message */}
      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4">
          <p className="text-red-600 dark:text-red-400">{error}</p>
        </div>
      )}

      {/* Plugin Form */}
      {isFormOpen && (
        <PluginForm
          onSubmit={handleSavePlugin}
          onCancel={() => setIsFormOpen(false)}
          isLoading={isLoading}
        />
      )}

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md">
            <div className="p-6">
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
                Confirm Delete
              </h3>
              <p className="text-gray-500 dark:text-gray-400 mb-6">
                Are you sure you want to delete the plugin "{pluginToDelete?.name}"? This action cannot be undone.
              </p>
              <div className="flex justify-end space-x-3">
                <Button
                  variant="secondary"
                  onClick={() => setIsDeleteConfirmOpen(false)}
                >
                  Cancel
                </Button>
                <Button
                  variant="danger"
                  onClick={handleDeletePlugin}
                >
                  Delete
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Plugin Table */}
      <PluginTable
        plugins={plugins}
        onEdit={handleEditPlugin}
        onDelete={handleDeleteClick}
        onSelect={handleSelectPlugin}
        isLoading={isLoading}
      />
    </div>
  );
};