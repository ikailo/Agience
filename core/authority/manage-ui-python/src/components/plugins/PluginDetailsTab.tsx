import React, { useState, useEffect, useCallback } from 'react';
import { pluginService } from '../../services/api/pluginService';
import { Plugin, PluginFormData} from '../../types/Plugin';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';
import PluginForm from './PluginForm';

interface PluginDetailsTabProps {
  selectedPluginId: string | null;
  onSelectPlugin: (id: string, switchToFunctions: boolean) => void;
}

/**
 * Component for managing plugins
 */
const PluginDetailsTab: React.FC<PluginDetailsTabProps> = ({
  selectedPluginId,
  onSelectPlugin
}) => {
  const [plugins, setPlugins] = useState<Plugin[]>([]);
  const [selectedPlugin, setSelectedPlugin] = useState<Plugin | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [editingPluginId, setEditingPluginId] = useState<string | null>(null);
  const [editFormData, setEditFormData] = useState<PluginFormData>({
    name: '',
    description: '',
    provider: 'Collection'
  });
  
  // Modal states
  const [notification, setNotification] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'info'
  });
  
  const [confirmation, setConfirmation] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    pluginId?: string;
    type: 'danger' | 'warning' | 'info';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'danger'
  });

  /**
   * Fetches all plugins
   */
  const fetchPlugins = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await pluginService.getAllPlugins();
      setPlugins(data);
    } catch (error) {
      console.error('Error fetching plugins:', error);
      showNotification('Error', 'Failed to load plugins', 'error');
    } finally {
      setIsLoading(false);
    }
  }, []);

  /**
   * Fetches a specific plugin by ID
   */
  const fetchPluginDetails = useCallback(async (id: string) => {
    try {
      setIsLoading(true);
      const plugin = await pluginService.getPluginById(id);
      setSelectedPlugin(plugin);
    } catch (error) {
      console.error('Error fetching plugin details:', error);
      showNotification('Error', 'Failed to load plugin details', 'error');
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Fetch plugins when component mounts
  useEffect(() => {
    fetchPlugins();
  }, [fetchPlugins]);

  // Fetch selected plugin details when selectedPluginId changes
  useEffect(() => {
    if (selectedPluginId) {
      fetchPluginDetails(selectedPluginId);
    } else {
      setSelectedPlugin(null);
    }
  }, [selectedPluginId, fetchPluginDetails]);

  /**
   * Shows a notification modal
   */
  const showNotification = (title: string, message: string, type: 'success' | 'error' | 'info' | 'warning') => {
    setNotification({
      isOpen: true,
      title,
      message,
      type
    });
  };

  /**
   * Closes the notification modal
   */
  const closeNotification = () => {
    setNotification(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Shows a confirmation modal for deleting a plugin
   */
  const showDeleteConfirmation = (plugin: Plugin) => {
    setConfirmation({
      isOpen: true,
      title: 'Delete Plugin',
      message: `Are you sure you want to delete the plugin "${plugin.name}"? This action cannot be undone.`,
      pluginId: plugin.id,
      type: 'danger'
    });
  };

  /**
   * Closes the confirmation modal
   */
  const closeConfirmation = () => {
    setConfirmation(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Handles plugin selection
   */
  const handleSelectPlugin = (plugin: Plugin) => {
    // When a plugin row is clicked, select it and switch to functions tab
    onSelectPlugin(plugin.id, true);
  };

  /**
   * Opens the form for creating a new plugin
   */
  const handleAddPlugin = () => {
    setEditFormData({
      name: '',
      description: '',
      provider: 'Collection'
    });
    setIsFormOpen(true);
  };

  /**
   * Starts in-cell editing for a plugin
   */
  const handleEditPlugin = (plugin: Plugin) => {
    setEditingPluginId(plugin.id);
    setEditFormData({
      name: plugin.name,
      description: plugin.description,
      provider: plugin.provider
    });
  };

  /**
   * Cancels in-cell editing
   */
  const handleCancelEdit = () => {
    setEditingPluginId(null);
  };

  /**
   * Handles input changes during in-cell editing
   */
  const handleEditInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setEditFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  /**
   * Saves changes from in-cell editing
   */
  const handleSaveEdit = async (pluginId: string) => {
    try {
      setIsLoading(true);
      const updatedPlugin = await pluginService.updatePlugin(pluginId, editFormData);
      showNotification('Success', 'Plugin updated successfully', 'success');
      
      // Update plugins list
      await fetchPlugins();
      
      // If this was the selected plugin, update it
      if (selectedPluginId === pluginId) {
        setSelectedPlugin(updatedPlugin);
      }
      
      // Exit edit mode
      setEditingPluginId(null);
    } catch (error) {
      console.error('Error updating plugin:', error);
      showNotification('Error', 'Failed to update plugin', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Closes the plugin form
   */
  const handleCloseForm = () => {
    setIsFormOpen(false);
  };

  /**
   * Handles saving a new plugin
   */
  const handleSavePlugin = async (formData: PluginFormData) => {
    try {
      setIsLoading(true);
      
      // Create new plugin
      const newPlugin = await pluginService.createPlugin(formData);
      showNotification('Success', 'Plugin created successfully', 'success');
      
      // Select the new plugin but stay on details tab
      setSelectedPlugin(newPlugin);
      onSelectPlugin(newPlugin.id, false);
      
      // Refresh plugins list
      await fetchPlugins();
      
      // Close form
      handleCloseForm();
    } catch (error) {
      console.error('Error saving plugin:', error);
      showNotification('Error', 'Failed to save plugin', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles confirming plugin deletion
   */
  const handleConfirmDelete = async () => {
    if (!confirmation.pluginId) return;
    
    try {
      setIsLoading(true);
      await pluginService.deletePlugin(confirmation.pluginId);
      
      showNotification('Success', 'Plugin deleted successfully', 'success');
      
      // If the deleted plugin was selected, clear selection
      if (selectedPluginId === confirmation.pluginId) {
        onSelectPlugin('', false);
      }
      
      // Refresh plugins list
      await fetchPlugins();
    } catch (error) {
      console.error('Error deleting plugin:', error);
      showNotification('Error', 'Failed to delete plugin', 'error');
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  /**
   * Gets badge color based on provider type
   */
  const getProviderBadgeColor = (provider: string) => {
    switch (provider) {
      case 'Collection':
        return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300';
      case 'Prompts':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300';
      case 'Semantic Kernel Plugin':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-white">
          Plugin Configuration
        </h2>
        <button
          onClick={handleAddPlugin}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
          disabled={isLoading}
        >
          Add Plugin
        </button>
      </div>

      {/* Plugin Form Modal */}
      {isFormOpen && (
        <PluginForm
          onSubmit={handleSavePlugin}
          onCancel={handleCloseForm}
          isLoading={isLoading}
        />
      )}

      {/* Plugins Table */}
      {isLoading && plugins.length === 0 ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-700">
            <thead>
              <tr>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Name
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Description
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Provider
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Created Date
                </th>
                <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-700">
              {plugins.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-4 text-center text-gray-300">
                    No plugins found. Add a plugin to get started.
                  </td>
                </tr>
              ) : (
                plugins.map((plugin) => (
                  <tr 
                    key={plugin.id} 
                    className={`hover:bg-gray-800 ${editingPluginId === plugin.id ? 'bg-gray-800' : ''}`}
                    onClick={() => editingPluginId !== plugin.id && handleSelectPlugin(plugin)}
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      {editingPluginId === plugin.id ? (
                        <input
                          type="text"
                          name="name"
                          value={editFormData.name}
                          onChange={handleEditInputChange}
                          className="w-full bg-gray-700 border-gray-600 text-white rounded px-2 py-1"
                          autoFocus
                        />
                      ) : (
                        <div className="flex items-center">
                          <div className="flex-shrink-0 h-10 w-10 flex items-center justify-center bg-gray-800 rounded-full">
                            <svg className="h-6 w-6 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                            </svg>
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-white">
                              {plugin.name}
                            </div>
                          </div>
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {editingPluginId === plugin.id ? (
                        <input
                          type="text"
                          name="description"
                          value={editFormData.description}
                          onChange={handleEditInputChange}
                          className="w-full bg-gray-700 border-gray-600 text-white rounded px-2 py-1"
                        />
                      ) : (
                        <div className="text-sm text-gray-300">
                          {plugin.description}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {editingPluginId === plugin.id ? (
                        <select
                          name="provider"
                          value={editFormData.provider}
                          onChange={handleEditInputChange}
                          className="bg-gray-700 border-gray-600 text-white rounded px-2 py-1"
                        >
                          <option value="Collection">Collection</option>
                          <option value="Prompts">Prompts</option>
                          <option value="Semantic Kernel Plugin">Semantic Kernel Plugin</option>
                        </select>
                      ) : (
                        <span className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${getProviderBadgeColor(plugin.provider)}`}>
                          {plugin.provider}
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                      {plugin.created_date ? new Date(plugin.created_date).toLocaleDateString() : '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      {editingPluginId === plugin.id ? (
                        <>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleSaveEdit(plugin.id);
                            }}
                            className="text-green-400 hover:text-green-300 mr-4"
                          >
                            Save
                          </button>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleCancelEdit();
                            }}
                            className="text-gray-400 hover:text-gray-300"
                          >
                            Cancel
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleEditPlugin(plugin);
                            }}
                            className="text-indigo-400 hover:text-indigo-300 mr-4"
                          >
                            Edit
                          </button>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              showDeleteConfirmation(plugin);
                            }}
                            className="text-red-400 hover:text-red-300"
                          >
                            Delete
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Notification Modal */}
      <NotificationModal
        isOpen={notification.isOpen}
        onClose={closeNotification}
        title={notification.title}
        message={notification.message}
        type={notification.type}
      />

      {/* Confirmation Modal */}
      <ConfirmationModal
        isOpen={confirmation.isOpen}
        onClose={closeConfirmation}
        onConfirm={handleConfirmDelete}
        title={confirmation.title}
        message={confirmation.message}
        confirmText="Delete"
        cancelText="Cancel"
        type="danger"
        isLoading={isLoading}
      />
    </div>
  );
};

export default PluginDetailsTab; 