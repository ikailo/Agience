import React, { useState, useEffect, useCallback } from 'react';
import { pluginService } from '../../services/api/pluginService';
import { Plugin, PluginFormData} from '../../types/Plugin';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';
import PluginForm from './PluginForm';

interface PluginDetailsTabProps {
  selectedPluginId: string | null;
  onSelectPlugin: (id: string, switchToFunctions: boolean) => void;
  onPluginChange?: () => Promise<void>;
  isCreatingNew?: boolean;
  onCancelCreate?: () => void;
}

/**
 * Component for managing plugins
 */
const PluginDetailsTab: React.FC<PluginDetailsTabProps> = ({
  selectedPluginId,
  onSelectPlugin,
  onPluginChange,
  isCreatingNew = false,
  onCancelCreate
}) => {
  const [selectedPlugin, setSelectedPlugin] = useState<Plugin | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
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

  // Fetch selected plugin details when selectedPluginId changes
  useEffect(() => {
    if (selectedPluginId) {
      fetchPluginDetails(selectedPluginId);
    } else {
      setSelectedPlugin(null);
    }
  }, [selectedPluginId, fetchPluginDetails]);

  // Open form when isCreatingNew changes to true
  useEffect(() => {
    if (isCreatingNew) {
      handleAddPlugin();
    }
  }, [isCreatingNew]);

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
   * Opens the form for editing the selected plugin
   */
  const handleEditPlugin = () => {
    if (!selectedPlugin) return;
    
    setEditFormData({
      name: selectedPlugin.name,
      description: selectedPlugin.description,
      provider: selectedPlugin.provider
    });
    setIsFormOpen(true);
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
   * Closes the plugin form
   */
  const handleCloseForm = () => {
    setIsFormOpen(false);
    if (isCreatingNew && onCancelCreate) {
      onCancelCreate();
    }
  };

  /**
   * Handles saving a plugin (create or update)
   */
  const handleSavePlugin = async (formData: PluginFormData) => {
    try {
      setIsLoading(true);
      
      if (selectedPlugin) {
        // Update existing plugin
        const updatedPlugin = await pluginService.updatePlugin(selectedPlugin.id, formData);
        setSelectedPlugin(updatedPlugin);
        showNotification('Success', 'Plugin updated successfully', 'success');
      } else {
        // Create new plugin
        const newPlugin = await pluginService.createPlugin(formData);
        setSelectedPlugin(newPlugin);
        onSelectPlugin(newPlugin.id, false);
        showNotification('Success', 'Plugin created successfully', 'success');
      }
      
      // Close form
      handleCloseForm();
      
      // Notify parent component about the change
      if (onPluginChange) {
        await onPluginChange();
      }
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
      
      // Clear selection
      onSelectPlugin('', false);
      setSelectedPlugin(null);
      
      // Notify parent component about the change
      if (onPluginChange) {
        await onPluginChange();
      }
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

  // If no plugin is selected and not creating a new one, show a message
  if (!selectedPluginId && !isFormOpen && !isCreatingNew) {
    return (
      <div className="flex flex-col items-center justify-center h-64 bg-gray-50 dark:bg-gray-800 rounded-lg">
        <svg className="h-16 w-16 text-gray-400 dark:text-gray-500 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M13 10V3L4 14h7v7l9-11h-7z" />
        </svg>
        <p className="text-gray-600 dark:text-gray-400 mb-4">Select a plugin from the list or create a new one</p>
        <button
          onClick={handleAddPlugin}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
        >
          Create New Plugin
        </button>
      </div>
    );
  }

  // Show loading state
  if (isLoading && !isFormOpen) {
    return (
      <div className="flex justify-center py-12">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-white">
          {selectedPlugin ? 'Plugin Details' : 'Create New Plugin'}
        </h2>
        {selectedPlugin && (
          <div className="flex space-x-3">
            <button
              onClick={handleEditPlugin}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
            >
              Edit Plugin
            </button>
            <button
              onClick={() => showDeleteConfirmation(selectedPlugin)}
              className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
            >
              Delete Plugin
            </button>
          </div>
        )}
      </div>

      {/* Plugin Form Modal */}
      {isFormOpen && (
        <PluginForm
          onSubmit={handleSavePlugin}
          onCancel={handleCloseForm}
          isLoading={isLoading}
          initialData={selectedPlugin ? {
            name: selectedPlugin.name,
            description: selectedPlugin.description,
            provider: selectedPlugin.provider
          } : undefined}
        />
      )}

      {/* Plugin Details */}
      {selectedPlugin && !isFormOpen && (
        <div className="bg-gray-800 rounded-lg p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h3 className="text-sm font-medium text-gray-400 mb-1">Name</h3>
              <p className="text-white text-lg">{selectedPlugin.name}</p>
            </div>
            <div>
              <h3 className="text-sm font-medium text-gray-400 mb-1">Provider</h3>
              <span className={`px-3 py-1 inline-flex text-sm leading-5 font-semibold rounded-full ${getProviderBadgeColor(selectedPlugin.provider)}`}>
                {selectedPlugin.provider}
              </span>
            </div>
            <div className="md:col-span-2">
              <h3 className="text-sm font-medium text-gray-400 mb-1">Description</h3>
              <p className="text-white">{selectedPlugin.description || 'No description provided'}</p>
            </div>
            <div>
              <h3 className="text-sm font-medium text-gray-400 mb-1">Created Date</h3>
              <p className="text-white">
                {selectedPlugin.created_date 
                  ? new Date(selectedPlugin.created_date).toLocaleDateString() 
                  : 'Not available'}
              </p>
            </div>
            <div>
              <h3 className="text-sm font-medium text-gray-400 mb-1">ID</h3>
              <p className="text-white text-sm font-mono bg-gray-700 p-2 rounded">{selectedPlugin.id}</p>
            </div>
          </div>
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