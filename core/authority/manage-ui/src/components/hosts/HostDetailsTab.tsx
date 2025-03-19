import { useState, useEffect } from 'react';
import Button from '../common/Button';
import { HostModal } from './HostModal';
import { HostTable } from './HostTable';
import { hostService } from '../../services/api/hostService';
import { Host } from '../../types/Host';

export const HostDetailsTab: React.FC = () => {
  const [hosts, setHosts] = useState<Host[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedHost, setSelectedHost] = useState<Host | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
  const [hostToDelete, setHostToDelete] = useState<Host | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    fetchHosts();
  }, []);

  const fetchHosts = async () => {
    try {
      setIsLoading(true);
      const data = await hostService.getAllHosts();
      setHosts(data);
      setError(null);
    } catch (err) {
      console.error('Error fetching hosts:', err);
      setError('Failed to load hosts. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddHost = () => {
    setSelectedHost(null);
    setIsEditing(false);
    setIsModalOpen(true);
  };

  const handleEditHost = (host: Host) => {
    setSelectedHost(host);
    setIsEditing(true);
    setIsModalOpen(true);
  };

  const handleDeleteClick = (host: Host) => {
    setHostToDelete(host);
    setIsDeleteConfirmOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedHost(null);
  };

  const handleDeleteHost = async () => {
    if (!hostToDelete) return;
    
    try {
      await hostService.deleteHost(hostToDelete.id);
      
      // Show success message
      setSuccessMessage('Host deleted successfully!');
      
      // Refresh the hosts list
      fetchHosts();
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
    } catch (err) {
      console.error('Error deleting host:', err);
      setError('Failed to delete host. Please try again.');
    } finally {
      setIsDeleteConfirmOpen(false);
      setHostToDelete(null);
    }
  };

  const handleSaveHost = async (formData: any): Promise<void> => {
    try {
      // If editing, update existing host
      if (isEditing && selectedHost) {
        await hostService.updateHost(selectedHost.id, formData);
      } else {
        // Otherwise create new host
        await hostService.createHost(formData);
      }
      
      // Show success message
      setSuccessMessage(isEditing ? 'Host updated successfully!' : 'Host created successfully!');
      
      // Refresh the hosts list
      fetchHosts();
      
      // Close the modal
      setIsModalOpen(false);
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
    } catch (err) {
      console.error('Error saving host:', err);
      setError('Failed to save host. Please try again.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Host Configuration
        </h2>
        <Button variant="primary" onClick={handleAddHost} disabled={isLoading}>
          {isLoading ? (
            <span className="flex items-center">
              <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Processing...
            </span>
          ) : (
            'Add Host'
          )}
        </Button>
      </div>

      {/* Success message */}
      {successMessage && (
        <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-4 animate-fadeIn">
          <p className="text-green-600 dark:text-green-400">{successMessage}</p>
        </div>
      )}

      {/* Error message */}
      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4">
          <p className="text-red-600 dark:text-red-400">{error}</p>
        </div>
      )}

      {/* Host Table */}
      <HostTable 
        hosts={hosts} 
        onEdit={handleEditHost} 
        onDelete={handleDeleteClick} 
        isLoading={isLoading && hosts.length === 0}
      />

      {/* Host Modal */}
      <HostModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        onSave={handleSaveHost}
        initialData={selectedHost ? {
          name: selectedHost.name,
          description: selectedHost.description,
          operatorId: selectedHost.operatorId,
          scopes: selectedHost.scopes
        } : undefined}
        isEditing={isEditing}
      />

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && hostToDelete && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md">
            <div className="p-6">
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
                Confirm Delete
              </h3>
              <p className="text-gray-500 dark:text-gray-400 mb-6">
                Are you sure you want to delete the host "{hostToDelete.name}"? This action cannot be undone.
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
                  onClick={handleDeleteHost}
                >
                  Delete
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}; 