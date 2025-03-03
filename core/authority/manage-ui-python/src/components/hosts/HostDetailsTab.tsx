import { useState, useEffect } from 'react';
import { HostModal } from './HostModal';
import { hostService } from '../../services/api/hostService';
import { HostFormData, Host } from '../../types/Host';

export const HostDetailsTab: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [hosts, setHosts] = useState<Host[]>([]);
  const [isLoadingHosts, setIsLoadingHosts] = useState(true);
  const [selectedHost, setSelectedHost] = useState<Host | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
  const [hostToDelete, setHostToDelete] = useState<Host | null>(null);

  // Fetch all hosts when component mounts
  useEffect(() => {
    fetchHosts();
  }, []);

  // Function to fetch all hosts
  const fetchHosts = async () => {
    try {
      setIsLoadingHosts(true);
      const hostsData = await hostService.getAllHosts();
      console.log('Hosts data:', hostsData); // Log the data to inspect it
      setHosts(hostsData);
      setError(null);
    } catch (err) {
      console.error('Error fetching hosts:', err);
      setError('Failed to load hosts. Please try again.');
    } finally {
      setIsLoadingHosts(false);
    }
  };

  // Function to handle saving a host (create or update)
  const handleSaveHost = async (hostData: HostFormData): Promise<void> => {
    try {
      setIsLoading(true);
      setError(null);
      
      if (isEditing && selectedHost) {
        // Update existing host
        await hostService.updateHost(selectedHost.id, hostData);
        setSuccessMessage('Host updated successfully!');
      } else {
        // Create new host
        await hostService.createHost(hostData);
        setSuccessMessage('Host created successfully!');
      }
      
      // Refresh the hosts list
      fetchHosts();
      
      // Reset editing state
      setIsEditing(false);
      setSelectedHost(null);
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
      
    } catch (err) {
      console.error('Error saving host:', err);
      setError(`Failed to ${isEditing ? 'update' : 'create'} host. Please try again.`);
    } finally {
      setIsLoading(false);
    }
  };

  // Function to handle editing a host
  const handleEditHost = (host: Host) => {
    setSelectedHost(host);
    setIsEditing(true);
    setIsModalOpen(true);
  };

  // Function to open delete confirmation
  const handleDeleteClick = (host: Host) => {
    setHostToDelete(host);
    setIsDeleteConfirmOpen(true);
  };

  // Function to handle deleting a host
  const handleDeleteHost = async () => {
    if (!hostToDelete) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      await hostService.deleteHost(hostToDelete.id);
      
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
      setIsLoading(false);
      setIsDeleteConfirmOpen(false);
      setHostToDelete(null);
    }
  };

  // Function to handle adding a new host
  const handleAddHost = () => {
    setSelectedHost(null);
    setIsEditing(false);
    setIsModalOpen(true);
  };

  // Function to close the modal
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedHost(null);
    setIsEditing(false);
  };

  return (
    <div className="space-y-6">
      {/* Header with Add Host button */}
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-medium text-gray-900 dark:text-white">
          Host Configuration
        </h2>
        <button
          onClick={handleAddHost}
          className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700"
          disabled={isLoading}
        >
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
        </button>
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

      {/* Host Modal */}
      <HostModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        onSave={handleSaveHost}
        initialData={selectedHost ? {
          name: selectedHost.name,
          address: selectedHost.address,
          operatorId: selectedHost.operatorId
        } : undefined}
        isEditing={isEditing}
      />

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md">
            <div className="p-6">
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
                Confirm Delete
              </h3>
              <p className="text-gray-500 dark:text-gray-400 mb-6">
                Are you sure you want to delete the host "{hostToDelete?.name}"? This action cannot be undone.
              </p>
              <div className="flex justify-end space-x-3">
                <button
                  onClick={() => setIsDeleteConfirmOpen(false)}
                  className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white dark:hover:bg-gray-600"
                >
                  Cancel
                </button>
                <button
                  onClick={handleDeleteHost}
                  disabled={isLoading}
                  className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 dark:bg-red-700 dark:hover:bg-red-800 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isLoading ? 'Deleting...' : 'Delete'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Host list */}
      {isLoadingHosts ? (
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
        </div>
      ) : hosts.length > 0 ? (
        <div className="space-y-4">
          {/* Desktop view */}
          <div className="hidden sm:block overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Address
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Created Date
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {hosts.map((host) => (
                  <tr key={host.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                      {host.name}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {host.address}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {new Date(host.created_date).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button 
                        onClick={() => handleEditHost(host)}
                        className="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300 mr-4"
                      >
                        Edit
                      </button>
                      <button 
                        onClick={() => handleDeleteClick(host)}
                        className="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile view */}
          <div className="sm:hidden space-y-4">
            {hosts.map((host) => (
              <div key={host.id} className="bg-white dark:bg-gray-800 shadow rounded-lg p-4">
                <h3 className="font-medium text-gray-900 dark:text-white">{host.name}</h3>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{host.address}</p>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Created: {new Date(host.created_date).toLocaleDateString()}
                </p>
                <div className="mt-4 flex justify-end space-x-4">
                  <button 
                    onClick={() => handleEditHost(host)}
                    className="text-blue-600 dark:text-blue-400"
                  >
                    Edit
                  </button>
                  <button 
                    onClick={() => handleDeleteClick(host)}
                    className="text-red-600 dark:text-red-400"
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-6">
          <p className="text-gray-500 dark:text-gray-400">
            No hosts configured yet. Click "Add Host" to create your first host.
          </p>
        </div>
      )}
    </div>
  );
}; 