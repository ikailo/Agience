import React, { useState, useEffect, useCallback } from 'react';
import { Connection, ConnectionFormData } from '../../types/Connection';
import { Authorizer } from '../../types/Authorizer';
import { connectionService } from '../../services/api/connectionService';
import { authorizerService } from '../../services/api/authorizerService';
import { dataService } from '../../services/api/dataService';
import ConnectionList from './ConnectionList';
import ConnectionForm from './ConnectionForm';
import ConnectionDetails from './ConnectionDetails';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';

/**
 * Component for managing connections
 */
const ConnectionsTab: React.FC = () => {
  const [connections, setConnections] = useState<Connection[]>([]);
  const [authorizers, setAuthorizers] = useState<Authorizer[]>([]);
  const [selectedConnectionId, setSelectedConnectionId] = useState<string | null>(null);
  const [selectedConnection, setSelectedConnection] = useState<Connection | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [hasTempConnection, setHasTempConnection] = useState<boolean>(false);

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
    connectionId?: string;
    type: 'danger' | 'warning' | 'info';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'danger'
  });

  /**
   * Fetches connections with authorizer data
   */
  const fetchConnections = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await dataService.getConnectionsWithAuthorizers();
      setConnections(data);
    } catch (error) {
      console.error('Error fetching connections:', error);
      showNotification('Error', 'Failed to load connections', 'error');
    } finally {
      setIsLoading(false);
    }
  }, []);

  /**
   * Fetches all authorizers
   */
  const fetchAuthorizers = useCallback(async () => {
    try {
      const data = await authorizerService.getAllAuthorizers();
      setAuthorizers(data);
    } catch (error) {
      console.error('Error fetching authorizers:', error);
      showNotification('Error', 'Failed to load authorizers', 'error');
    }
  }, []);

  /**
   * Fetches a specific connection by ID with its authorizer
   */
  const fetchConnectionDetails = useCallback(async (id: string) => {
    try {
      setIsLoading(true);
      const connection = await dataService.getConnectionWithAuthorizer(id);
      setSelectedConnection(connection);
    } catch (error) {
      console.error('Error fetching connection details:', error);
      showNotification('Error', 'Failed to load connection details', 'error');
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Fetch connections and authorizers when component mounts
  useEffect(() => {
    fetchConnections();
    fetchAuthorizers();
  }, [fetchConnections, fetchAuthorizers]);

  // Fetch selected connection details when selectedConnectionId changes
  useEffect(() => {
    if (selectedConnectionId) {
      fetchConnectionDetails(selectedConnectionId);
    } else {
      setSelectedConnection(null);
    }
  }, [selectedConnectionId, fetchConnectionDetails]);

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
   * Shows a confirmation modal for deleting a connection
   */
  const showDeleteConfirmation = (connection: Connection) => {
    setConfirmation({
      isOpen: true,
      title: 'Delete Connection',
      message: `Are you sure you want to delete the connection "${connection.name}"? This action cannot be undone.`,
      connectionId: connection.id,
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
   * Handles connection selection
   */
  const handleSelectConnection = (id: string) => {
    setSelectedConnectionId(id);
  };

  /**
   * Opens the form for creating a new connection
   */
  const handleAddConnection = () => {
    setSelectedConnectionId(null);
    setSelectedConnection(null);
    setIsFormOpen(true);
    setHasTempConnection(true);
  };

  /**
   * Opens the form for editing the selected connection
   */
  const handleEditConnection = () => {
    if (!selectedConnection) return;
    setIsFormOpen(true);
  };

  /**
   * Closes the connection form
   */
  const handleCloseForm = () => {
    setIsFormOpen(false);
    setHasTempConnection(false);
  };

  /**
   * Handles saving a connection (create or update)
   */
  const handleSaveConnection = async (formData: ConnectionFormData) => {
    try {
      setIsLoading(true);
      
      if (selectedConnection) {
        // Update existing connection
        await connectionService.updateConnection(selectedConnection.id, formData);
        showNotification('Success', 'Connection updated successfully', 'success');
      } else {
        // Create new connection
        const newConnection = await connectionService.createConnection(formData);
        setSelectedConnectionId(newConnection.id);
        showNotification('Success', 'Connection created successfully', 'success');
      }
      
      // Refresh connections list with authorizer data
      await fetchConnections();
      
      // If we have a selected connection, refresh its details
      if (selectedConnectionId) {
        await fetchConnectionDetails(selectedConnectionId);
      }
      
      // Close form
      handleCloseForm();
    } catch (error) {
      console.error('Error saving connection:', error);
      showNotification('Error', 'Failed to save connection', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles confirming connection deletion
   */
  const handleConfirmDelete = async () => {
    if (!confirmation.connectionId) return;
    
    try {
      setIsLoading(true);
      await connectionService.deleteConnection(confirmation.connectionId);
      
      showNotification('Success', 'Connection deleted successfully', 'success');
      
      // Clear selection
      setSelectedConnectionId(null);
      setSelectedConnection(null);
      
      // Refresh connections list
      await fetchConnections();
    } catch (error) {
      console.error('Error deleting connection:', error);
      showNotification('Error', 'Failed to delete connection', 'error');
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  return (
    <div className="flex flex-col md:flex-row h-full gap-6">
      {/* Left sidebar with connection list */}
      <div className="w-full md:w-1/4 mb-6 md:mb-0">
        <ConnectionList
          connections={connections}
          selectedConnectionId={selectedConnectionId}
          isLoading={isLoading}
          onSelectConnection={handleSelectConnection}
          onCreateConnection={handleAddConnection}
          hasTempConnection={hasTempConnection}
        />
      </div>
      
      {/* Main content area */}
      <div className="w-full md:w-3/4">
        {isFormOpen ? (
          <ConnectionForm
            onSubmit={handleSaveConnection}
            onCancel={handleCloseForm}
            isLoading={isLoading}
            initialData={selectedConnection ? {
              name: selectedConnection.name,
              description: selectedConnection.description,
              authorizer_id: selectedConnection.authorizer_id
            } : undefined}
            authorizers={authorizers}
          />
        ) : selectedConnection ? (
          <ConnectionDetails
            connection={selectedConnection}
            onEdit={handleEditConnection}
            onDelete={() => showDeleteConfirmation(selectedConnection)}
          />
        ) : (
          <div className="flex flex-col items-center justify-center h-64 p-5 text-center md:p-0 lg:p-0 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <svg className="h-16 w-16 text-gray-400 dark:text-gray-500 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            <p className="text-gray-600 dark:text-gray-400 mb-4">Select a connection from the list or create a new one</p>
            <button
              onClick={handleAddConnection}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
            >
              Create New Connection
            </button>
          </div>
        )}
      </div>

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

export default ConnectionsTab; 