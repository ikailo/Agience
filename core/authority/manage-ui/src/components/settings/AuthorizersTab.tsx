import React, { useState, useEffect, useCallback } from 'react';
import { Authorizer, AuthorizerFormData } from '../../types/Authorizer';
import { authorizerService } from '../../services/api/authorizerService';
import { dataService } from '../../services/api/dataService';
import AuthorizerList from './AuthorizerList';
import AuthorizerForm from './AuthorizerForm';
import AuthorizerDetails from './AuthorizerDetails';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';

interface AuthorizersTabProps {
  onAuthorizersChange: (hasAuthorizers: boolean) => void;
}

/**
 * Component for managing authorizers
 */
const AuthorizersTab: React.FC<AuthorizersTabProps> = ({
  onAuthorizersChange
}) => {
  const [authorizers, setAuthorizers] = useState<Authorizer[]>([]);
  const [selectedAuthorizerId, setSelectedAuthorizerId] = useState<string | null>(null);
  const [selectedAuthorizer, setSelectedAuthorizer] = useState<Authorizer | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [hasTempAuthorizer, setHasTempAuthorizer] = useState<boolean>(false);

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
    authorizerId?: string;
    type: 'danger' | 'warning' | 'info';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'danger'
  });

  /**
   * Fetches all authorizers
   */
  const fetchAuthorizers = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await authorizerService.getAllAuthorizers();
      setAuthorizers(data);
      onAuthorizersChange(data.length > 0);
    } catch (error) {
      console.error('Error fetching authorizers:', error);
      showNotification('Error', 'Failed to load authorizers', 'error');
    } finally {
      setIsLoading(false);
    }
  }, [onAuthorizersChange]);

  /**
   * Fetches a specific authorizer by ID with its connections
   */
  const fetchAuthorizerDetails = useCallback(async (id: string) => {
    try {
      setIsLoading(true);
      const { authorizer } = await dataService.getAuthorizerWithConnections(id);
      setSelectedAuthorizer(authorizer);
    } catch (error) {
      console.error('Error fetching authorizer details:', error);
      showNotification('Error', 'Failed to load authorizer details', 'error');
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Fetch authorizers when component mounts
  useEffect(() => {
    fetchAuthorizers();
  }, [fetchAuthorizers]);

  // Fetch selected authorizer details when selectedAuthorizerId changes
  useEffect(() => {
    if (selectedAuthorizerId) {
      fetchAuthorizerDetails(selectedAuthorizerId);
    } else {
      setSelectedAuthorizer(null);
    }
  }, [selectedAuthorizerId, fetchAuthorizerDetails]);

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
   * Shows a confirmation modal for deleting an authorizer
   */
  const showDeleteConfirmation = (authorizer: Authorizer) => {
    setConfirmation({
      isOpen: true,
      title: 'Delete Authorizer',
      message: `Are you sure you want to delete the authorizer "${authorizer.name}"? This action cannot be undone.`,
      authorizerId: authorizer.id,
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
   * Handles authorizer selection
   */
  const handleSelectAuthorizer = (id: string) => {
    setSelectedAuthorizerId(id);
  };

  /**
   * Opens the form for creating a new authorizer
   */
  const handleAddAuthorizer = () => {
    setSelectedAuthorizerId(null);
    setSelectedAuthorizer(null);
    setIsFormOpen(true);
    setHasTempAuthorizer(true);
  };

  /**
   * Opens the form for editing the selected authorizer
   */
  const handleEditAuthorizer = () => {
    if (!selectedAuthorizer) return;
    setIsFormOpen(true);
  };

  /**
   * Closes the authorizer form
   */
  const handleCloseForm = () => {
    setIsFormOpen(false);
    setHasTempAuthorizer(false);
  };

  /**
   * Handles saving an authorizer (create or update)
   */
  const handleSaveAuthorizer = async (formData: AuthorizerFormData) => {
    try {
      setIsLoading(true);
      
      if (selectedAuthorizer) {
        // Update existing authorizer
        await authorizerService.updateAuthorizer(selectedAuthorizer.id, formData);
        showNotification('Success', 'Authorizer updated successfully', 'success');
      } else {
        // Create new authorizer
        const newAuthorizer = await authorizerService.createAuthorizer(formData);
        setSelectedAuthorizerId(newAuthorizer.id);
        showNotification('Success', 'Authorizer created successfully', 'success');
      }
      
      // Refresh authorizers list
      await fetchAuthorizers();
      
      // If we have a selected authorizer, refresh its details
      if (selectedAuthorizerId) {
        await fetchAuthorizerDetails(selectedAuthorizerId);
      }
      
      // Close form
      handleCloseForm();
    } catch (error) {
      console.error('Error saving authorizer:', error);
      showNotification('Error', 'Failed to save authorizer', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles confirming authorizer deletion
   */
  const handleConfirmDelete = async () => {
    if (!confirmation.authorizerId) return;
    
    try {
      setIsLoading(true);
      await authorizerService.deleteAuthorizer(confirmation.authorizerId);
      
      showNotification('Success', 'Authorizer deleted successfully', 'success');
      
      // Clear selection
      setSelectedAuthorizerId(null);
      setSelectedAuthorizer(null);
      
      // Refresh authorizers list
      await fetchAuthorizers();
    } catch (error) {
      console.error('Error deleting authorizer:', error);
      showNotification('Error', 'Failed to delete authorizer', 'error');
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  return (
    <div className="flex flex-col md:flex-row h-full gap-6">
      {/* Left sidebar with authorizer list */}
      <div className="w-full md:w-1/4 mb-6 md:mb-0">
        <AuthorizerList
          authorizers={authorizers}
          selectedAuthorizerId={selectedAuthorizerId}
          isLoading={isLoading}
          onSelectAuthorizer={handleSelectAuthorizer}
          onCreateAuthorizer={handleAddAuthorizer}
          hasTempAuthorizer={hasTempAuthorizer}
        />
      </div>
      
      {/* Main content area */}
      <div className="w-full md:w-3/4">
        {isFormOpen ? (
          <AuthorizerForm
            onSubmit={handleSaveAuthorizer}
            onCancel={handleCloseForm}
            isLoading={isLoading}
            initialData={selectedAuthorizer ? {
              name: selectedAuthorizer.name,
              type: selectedAuthorizer.type
            } : undefined}
          />
        ) : selectedAuthorizer ? (
          <AuthorizerDetails
            authorizer={selectedAuthorizer}
            onEdit={handleEditAuthorizer}
            onDelete={() => showDeleteConfirmation(selectedAuthorizer)}
          />
        ) : (
          <div className="flex flex-col items-center justify-center p-5 text-center md:p-0 lg:p-0 h-64 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <svg className="h-16 w-16 text-gray-400 dark:text-gray-500 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
            <p className="text-gray-600 dark:text-gray-400 mb-4">Select an authorizer from the list or create a new one</p>
            <button
              onClick={handleAddAuthorizer}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
            >
              Create New Authorizer
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

export default AuthorizersTab; 