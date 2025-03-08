import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { hostKeyService, Key, KeyFormData } from '../../services/api/hostKeyService';
import { hostService } from '../../services/api/hostService';
import { Host } from '../../types/Host';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';
import KeyDetailsModal from './KeyDetailsModal';
import Card from '../common/Card';
import Button from '../common/Button';
import Badge from '../common/Badge';
import FormField from '../common/FormField';
import DataTable from '../common/DataTable';

interface HostKeysProps {
  hostId?: string;
}

/**
 * HostKeys component that displays and manages keys for a host
 * Supports both light and dark modes and is mobile responsive
 */
function HostKeys({ hostId: propHostId }: HostKeysProps) {
  const [searchParams] = useSearchParams();
  const urlHostId = searchParams.get('id');
  
  // Use the prop hostId if provided, otherwise use the URL parameter
  const hostId = propHostId || urlHostId;
  
  const [host, setHost] = useState<Host | null>(null);
  const [keys, setKeys] = useState<Key[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  
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
    keyId?: string;
    type: 'danger' | 'warning' | 'info';
  }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'danger'
  });
  
  // Form state
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [formData, setFormData] = useState<KeyFormData>({
    name: '',
    isActive: true
  });
  
  // Edit state
  const [editingKey, setEditingKey] = useState<Key | null>(null);
  
  // Key details modal state
  const [selectedKey, setSelectedKey] = useState<Key | null>(null);
  const [isKeyDetailsModalOpen, setIsKeyDetailsModalOpen] = useState<boolean>(false);

  /**
   * Fetches the host details
   */
  const fetchHostDetails = useCallback(async () => {
    if (!hostId) return;
    
    try {
      const hostData = await hostService.getHostById(hostId);
      setHost(hostData);
    } catch (err) {
      console.error('Error fetching host details:', err);
      showNotification('Error', 'Failed to load host details', 'error');
    }
  }, [hostId]);

  /**
   * Fetches keys for the host
   */
  const fetchKeys = useCallback(async () => {
    if (!hostId) {
      // console.log('No hostId provided, skipping key fetch');
     //TODO: display a message to the user that no host is selected
      return;
    }
    
    console.log(`Attempting to fetch keys for host ID: ${hostId}`);
    
    try {
      setIsLoading(true);
      
      const keysData = await hostKeyService.getKeysForHost(hostId);
      // console.log('Fetched keys:', keysData);
      
      if (keysData.length === 0) {
        console.log('No keys found for this host');
      }
      
      // Log each key's properties for debugging
      keysData.forEach((key, index) => {
        console.log(`Key ${index}:`, {
          id: key.id,
          name: key.name,
          isActive: key.isActive,
          type: typeof key.isActive
        });
      });
      
      setKeys(keysData);
    } catch (err: any) {
      console.error('Error fetching keys:', err);
      
      // More detailed error logging
      if (err.response) {
          // console.error('Error response data:', err.response.data);
          // console.error('Error response status:', err.response.status);
          // console.error('Error response headers:', err.response.headers);
        showNotification('Error', `Failed to load keys: ${err.response.status} ${err.response.statusText}`, 'error');
      } else if (err.request) {
        // console.error('Error request:', err.request);
        showNotification('Error', 'Failed to load keys: No response received from server', 'error');
      } else {
        // console.error('Error message:', err.message);
        showNotification('Error', `Failed to load keys: ${err.message}`, 'error');
      }
      
      setKeys([]);
    } finally {
      setIsLoading(false);
    }
  }, [hostId]);

  // Fetch host details and keys when component mounts or hostId changes
  useEffect(() => {
    if (hostId) {
      fetchHostDetails();
      fetchKeys();
    } else {
      // Reset state when no host is selected
      setHost(null);
      setKeys([]);
    }
  }, [hostId, fetchHostDetails, fetchKeys]);

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
   * Shows a confirmation modal
   */
  const showConfirmation = (title: string, message: string, keyId: string, type: 'danger' | 'warning' | 'info' = 'danger') => {
    setConfirmation({
      isOpen: true,
      title,
      message,
      keyId,
      type
    });
  };

  /**
   * Closes the confirmation modal
   */
  const closeConfirmation = () => {
    setConfirmation(prev => ({ ...prev, isOpen: false }));
  };

  /**
   * Handles form input changes
   */
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  /**
   * Opens the form for creating a new key
   */
  const handleOpenForm = () => {
    setFormData({
      name: '',
      isActive: true
    });
    setEditingKey(null);
    setIsFormOpen(true);
  };

  /**
   * Opens the form for editing an existing key
   */
  const handleEditKey = (key: Key) => {
    // Ensure isActive is a boolean
    const isActive = typeof key.isActive === 'boolean' ? key.isActive : false;
    
    setFormData({
      name: key.name,
      isActive
    });
    setEditingKey(key);
    setIsFormOpen(true);
  };

  /**
   * Closes the form
   */
  const handleCloseForm = () => {
    setIsFormOpen(false);
    setEditingKey(null);
  };

  /**
   * Handles form submission for creating or updating a key
   */
  const handleSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!hostId) return;
    
    try {
      setIsLoading(true);
      
      if (editingKey) {
        // Update existing key
        await hostKeyService.updateKey(hostId, editingKey.id, {
          name: formData.name,
          isActive: formData.isActive
        });
        
        showNotification('Success', 'Key updated successfully', 'success');
      } else {
        // Generate new key
        const newKeyData = await hostKeyService.generateKeyForHost(hostId, formData);
        setSelectedKey(newKeyData);
        setIsKeyDetailsModalOpen(true);
      }
      
      // Refresh keys
      await fetchKeys();
      
      // Close form
      handleCloseForm();
    } catch (err) {
      console.error('Error saving key:', err);
      showNotification('Error', 'Failed to save key', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles confirming key deletion
   */
  const handleConfirmDelete = async () => {
    if (!hostId || !confirmation.keyId) return;
    
    try {
      setIsLoading(true);
      await hostKeyService.deleteKey(hostId, confirmation.keyId);
      
      showNotification('Success', 'Key deleted successfully', 'success');
      
      // Refresh keys
      await fetchKeys();
    } catch (err) {
      console.error('Error deleting key:', err);
      showNotification('Error', 'Failed to delete key', 'error');
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  /**
   * Opens the key details modal for a specific key
   */
  const handleViewKey = (key: Key) => {
    setSelectedKey(key);
    setIsKeyDetailsModalOpen(true);
  };

  /**
   * Closes the key details modal
   */
  const handleCloseKeyDetailsModal = () => {
    setIsKeyDetailsModalOpen(false);
    setSelectedKey(null);
  };

  // Define table columns
  const columns = [
    {
      header: 'Name',
      accessor: (key: Key) => (
        <div>
          <div className="text-sm font-medium text-gray-900 dark:text-white">
            {key.name}
          </div>
          <div className="text-xs text-gray-500 dark:text-gray-400">
            ID: {key.id}
          </div>
        </div>
      )
    },
    {
      header: 'Created At',
      accessor: (key: Key) => (
        <div className="text-sm text-gray-500 dark:text-gray-400">
          {new Date(key.created_date).toLocaleString()}
        </div>
      )
    },
    {
      header: 'Status',
      accessor: (key: Key) => (
        <Badge 
          variant={key.isActive ? 'success' : 'danger'} 
          rounded
        >
          {key.isActive ? 'Active' : 'Inactive'}
        </Badge>
      )
    },
    {
      header: 'Actions',
      accessor: (key: Key) => (
        <div className="flex justify-end space-x-2">
          <Button 
            variant="outline" 
            size="sm" 
            onClick={(e) => {
              e.stopPropagation();
              handleViewKey(key);
            }}
          >
            View
          </Button>
          <Button 
            variant="primary" 
            size="sm" 
            onClick={(e) => {
              e.stopPropagation();
              handleEditKey(key);
            }}
          >
            Edit
          </Button>
          <Button 
            variant="danger" 
            size="sm" 
            onClick={(e) => {
              e.stopPropagation();
              showConfirmation(
                'Delete Key',
                `Are you sure you want to delete the key "${key.name}"? This action cannot be undone.`,
                key.id
              );
            }}
          >
            Delete
          </Button>
        </div>
      ),
      className: 'text-right'
    }
  ];

  // If no host ID is provided, show a message
  if (!hostId) {
    return (
      <Card>
        <div className="text-center">
          <p className="text-gray-600 dark:text-gray-300">
            Please select a host from the Details tab first.
          </p>
        </div>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          {host ? `Keys for ${host.name}` : 'Host Keys'}
        </h2>
        <Button
          variant="primary"
          size="md"
          onClick={handleOpenForm}
          disabled={isLoading}
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
          }
        >
          Generate New Key
        </Button>
      </div>

      {/* Key Form */}
      {isFormOpen && (
        <Card
          title={editingKey ? 'Edit Key' : 'Generate New Key'}
          variant="bordered"
        >
          <form onSubmit={handleSubmitForm}>
            <FormField
              label="Name"
              htmlFor="name"
              required
            >
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                className="w-full px-3 py-2 bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                required
              />
            </FormField>
            
            <FormField
              label="Status"
              htmlFor="isActive"
            >
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isActive"
                  name="isActive"
                  checked={formData.isActive}
                  onChange={handleInputChange}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="isActive" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                  Active
                </label>
              </div>
            </FormField>
            
            <div className="flex justify-end space-x-3 mt-6">
              <Button
                variant="secondary"
                type="button"
                onClick={handleCloseForm}
              >
                Cancel
              </Button>
              <Button
                variant="primary"
                type="submit"
                isLoading={isLoading}
              >
                {editingKey ? 'Update Key' : 'Generate Key'}
              </Button>
            </div>
          </form>
        </Card>
      )}

      {/* Keys Table */}
      <DataTable
        columns={columns}
        data={keys}
        keyExtractor={(key) => key.id}
        isLoading={isLoading}
        emptyMessage="No keys found for this host. Generate a new key to get started."
        onRowClick={handleViewKey}
      />

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

      {/* Key Details Modal */}
      <KeyDetailsModal
        isOpen={isKeyDetailsModalOpen}
        onClose={handleCloseKeyDetailsModal}
        keyData={selectedKey}
      />
    </div>
  );
}

export default HostKeys; 