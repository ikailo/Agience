import React, { useState, useEffect, useCallback } from 'react';
import pluginFunctionService, { Function, FunctionFormData } from '../../services/api/pluginFunctionService';
import { pluginService } from '../../services/api/pluginService';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';
import FunctionForm from './functions/FunctionForm';

interface PluginFunctionsTabProps {
  pluginId: string;
}

/**
 * Component for managing functions associated with a plugin
 */
const PluginFunctionsTab: React.FC<PluginFunctionsTabProps> = ({ pluginId }) => {
  const [functions, setFunctions] = useState<Function[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [editingFunctionId, setEditingFunctionId] = useState<string | null>(null);
  const [editFormData, setEditFormData] = useState<FunctionFormData>({
    name: '',
    description: ''
  });
  const [pluginName, setPluginName] = useState<string>('');
  
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
    functionId?: string;
    type: 'danger' | 'warning' | 'info';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'danger'
  });

  /**
   * Fetches plugin details
   */
  const fetchPluginDetails = useCallback(async () => {
    if (!pluginId) return;
    
    try {
      const plugin = await pluginService.getPluginById(pluginId);
      setPluginName(plugin.name);
    } catch (error) {
      console.error('Error fetching plugin details:', error);
      showNotification('Error', 'Failed to load plugin details', 'error');
    }
  }, [pluginId]);

  /**
   * Fetches functions for the plugin
   */
  const fetchFunctions = useCallback(async () => {
    if (!pluginId) return;
    
    try {
      setIsLoading(true);
      const data = await pluginFunctionService.getFunctionsForPlugin(pluginId);
      setFunctions(data);
    } catch (error) {
      console.error('Error fetching functions:', error);
      showNotification('Error', 'Failed to load functions', 'error');
    } finally {
      setIsLoading(false);
    }
  }, [pluginId]);

  // Fetch data when component mounts or pluginId changes
  useEffect(() => {
    if (pluginId) {
      fetchPluginDetails();
      fetchFunctions();
    }
  }, [pluginId, fetchPluginDetails, fetchFunctions]);

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
   * Shows a confirmation modal for deleting a function
   */
  const showDeleteConfirmation = (func: Function) => {
    setConfirmation({
      isOpen: true,
      title: 'Delete Function',
      message: `Are you sure you want to delete the function "${func.name}"? This action cannot be undone.`,
      functionId: func.id,
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
   * Opens the form for creating a new function
   */
  const handleAddFunction = () => {
    setIsFormOpen(true);
  };

  /**
   * Starts in-cell editing for a function
   */
  const handleEditFunction = (func: Function) => {
    setEditingFunctionId(func.id);
    setEditFormData({
      name: func.name,
      description: func.description
    });
  };

  /**
   * Cancels in-cell editing
   */
  const handleCancelEdit = () => {
    setEditingFunctionId(null);
  };

  /**
   * Handles input changes during in-cell editing
   */
  const handleEditInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setEditFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  /**
   * Saves changes from in-cell editing
   */
  const handleSaveEdit = async (functionId: string) => {
    try {
      setIsLoading(true);
      await pluginFunctionService.updateFunction(functionId, editFormData);
      showNotification('Success', 'Function updated successfully', 'success');
      
      // Refresh functions list
      await fetchFunctions();
      
      // Exit edit mode
      setEditingFunctionId(null);
    } catch (error) {
      console.error('Error updating function:', error);
      showNotification('Error', 'Failed to update function', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Closes the function form
   */
  const handleCloseForm = () => {
    setIsFormOpen(false);
  };

  /**
   * Handles saving a new function
   */
  const handleSaveFunction = async (formData: FunctionFormData) => {
    try {
      setIsLoading(true);
      
      // Create new function
      await pluginFunctionService.createFunctionForPlugin(pluginId, formData);
      showNotification('Success', 'Function created successfully', 'success');
      
      // Refresh functions list
      await fetchFunctions();
      
      // Close form
      handleCloseForm();
    } catch (error) {
      console.error('Error saving function:', error);
      showNotification('Error', 'Failed to save function', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles confirming function deletion
   */
  const handleConfirmDelete = async () => {
    if (!confirmation.functionId) return;
    
    try {
      setIsLoading(true);
      
      try {
        // First remove the function from the plugin
        await pluginFunctionService.removeFunctionFromPlugin(pluginId, confirmation.functionId);
        
        try {
          // Then try to delete the function itself
          await pluginFunctionService.deleteFunction(confirmation.functionId);
        } catch (deleteError: any) {
          // Function gets deleted however shows a 404 error
          // a placeholder to handle this, NEED TO FIX LATER
          if (deleteError.response && deleteError.response.status !== 404) {
            // Only rethrow if it's not a 404 error
            throw deleteError;
          }
          console.log('Function was removed from plugin but might already be deleted');
        }
      } catch (error) {
        throw error; 
      }
      
      showNotification('Success', 'Function deleted successfully', 'success');
      
      // Refresh functions list
      await fetchFunctions();
    } catch (error) {
      console.error('Error deleting function:', error);
      showNotification('Error', 'Failed to delete function', 'error');
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  // If no plugin ID is provided, show a message
  if (!pluginId) {
    return (
      <div className="p-6 text-center text-gray-300">
        Please select a plugin first.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-white">
          {pluginName ? `Functions for ${pluginName}` : 'Plugin Functions'}
        </h2>
        <button
          onClick={handleAddFunction}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
          disabled={isLoading}
        >
          Add Function
        </button>
      </div>

      {/* Function Form Modal */}
      {isFormOpen && (
        <FunctionForm
          onSubmit={handleSaveFunction}
          onCancel={handleCloseForm}
          isLoading={isLoading}
        />
      )}

      {/* Functions Table */}
      {isLoading && functions.length === 0 ? (
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
                <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-700">
              {functions.length === 0 ? (
                <tr>
                  <td colSpan={3} className="px-6 py-4 text-center text-gray-300">
                    No functions found for this plugin. Add a function to get started.
                  </td>
                </tr>
              ) : (
                functions.map((func) => (
                  <tr 
                    key={func.id} 
                    className={`hover:bg-gray-800 ${editingFunctionId === func.id ? 'bg-gray-800' : ''}`}
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      {editingFunctionId === func.id ? (
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
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
                            </svg>
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-white">
                              {func.name}
                            </div>
                          </div>
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {editingFunctionId === func.id ? (
                        <input
                          type="text"
                          name="description"
                          value={editFormData.description}
                          onChange={handleEditInputChange}
                          className="w-full bg-gray-700 border-gray-600 text-white rounded px-2 py-1"
                        />
                      ) : (
                        <div className="text-sm text-gray-300">
                          {func.description}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      {editingFunctionId === func.id ? (
                        <>
                          <button
                            onClick={() => handleSaveEdit(func.id)}
                            className="text-green-400 hover:text-green-300 mr-4"
                          >
                            Save
                          </button>
                          <button
                            onClick={handleCancelEdit}
                            className="text-gray-400 hover:text-gray-300"
                          >
                            Cancel
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            onClick={() => handleEditFunction(func)}
                            className="text-indigo-400 hover:text-indigo-300 mr-4"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => showDeleteConfirmation(func)}
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

export default PluginFunctionsTab; 