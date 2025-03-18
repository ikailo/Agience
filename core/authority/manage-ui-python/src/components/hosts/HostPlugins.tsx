import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Plugin } from '../../types/Plugin';
import { Host } from '../../types/Host';
import { hostPluginService } from '../../services/api/hostPluginService';
import { hostService } from '../../services/api/hostService';
import { pluginService } from '../../services/api/pluginService';
import NotificationModal from '../common/NotificationModal';
import ConfirmationModal from '../common/ConfirmationModal';
import Button from '../common/Button';

interface HostPluginsProps {
  hostId?: string;
}

const HostPlugins = ({ hostId: propHostId }: HostPluginsProps) => {
  const [searchParams] = useSearchParams();
  const urlHostId = searchParams.get('id');
  const hostId = propHostId || urlHostId;

  const [host, setHost] = useState<Host | null>(null);
  const [assignedPlugins, setAssignedPlugins] = useState<Plugin[]>([]);
  const [availablePlugins, setAvailablePlugins] = useState<Plugin[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [notification, setNotification] = useState<{ isOpen: boolean; title: string; message: string; type: 'success' | 'error' | 'info' | 'warning' }>({
    isOpen: false,
    title: '',
    message: '',
    type: 'info'
  });
  const [confirmation, setConfirmation] = useState<{ isOpen: boolean; pluginId: string; pluginName: string }>({
    isOpen: false,
    pluginId: '',
    pluginName: ''
  });

  // Fetch host details
  const fetchHostDetails = useCallback(async () => {
    if (!hostId) return;
    try {
      const hostData = await hostService.getHostById(hostId);
      setHost(hostData);
    } catch (error) {
      console.error('Error fetching host details:', error);
      showNotification('Error', 'Failed to load host details', 'error');
    }
  }, [hostId]);

  // Fetch assigned plugins
  const fetchAssignedPlugins = useCallback(async () => {
    if (!hostId) return;
    try {
      const plugins = await hostPluginService.getPluginsForHost(hostId);
      setAssignedPlugins(plugins);
    } catch (error) {
      console.error('Error fetching assigned plugins:', error);
      showNotification('Error', 'Failed to load assigned plugins', 'error');
    }
  }, [hostId]);

  // Fetch available plugins
  const fetchAvailablePlugins = useCallback(async () => {
    try {
      const plugins = await pluginService.getAllPlugins();
      setAvailablePlugins(plugins);
    } catch (error) {
      console.error('Error fetching available plugins:', error);
      showNotification('Error', 'Failed to load available plugins', 'error');
    }
  }, []);

  // Initial data fetch
  useEffect(() => {
    const fetchData = async () => {
      if (!hostId) return;
      
      setIsLoading(true);
      try {
        await Promise.all([
          fetchHostDetails(),
          fetchAssignedPlugins(),
          fetchAvailablePlugins()
        ]);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchData();
    
    // Cleanup function for when component unmounts or hostId changes
    return () => {
      // Reset state on component unmount
      setHost(null);
      setAssignedPlugins([]);
      setAvailablePlugins([]);
    };
  }, [hostId, fetchHostDetails, fetchAssignedPlugins, fetchAvailablePlugins]);

  const showNotification = (title: string, message: string, type: 'success' | 'error' | 'info' | 'warning') => {
    setNotification({ isOpen: true, title, message, type });
  };

  const closeNotification = () => {
    setNotification(prev => ({ ...prev, isOpen: false }));
  };

  const showRemoveConfirmation = (pluginId: string, pluginName: string) => {
    setConfirmation({ isOpen: true, pluginId, pluginName });
  };

  const closeConfirmation = () => {
    setConfirmation(prev => ({ ...prev, isOpen: false }));
  };

  const handleAddPlugin = async (pluginId: string) => {
    if (!hostId) return;
    setIsLoading(true);
    try {
      await hostPluginService.addPluginToHost(hostId, pluginId);
      await fetchAssignedPlugins();
      const plugin = availablePlugins.find(p => p.id === pluginId);
      showNotification('Success', `Plugin "${plugin?.name}" added successfully`, 'success');
    } catch (error) {
      console.error('Error adding plugin:', error);
      showNotification('Error', 'Failed to add plugin', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRemovePlugin = async () => {
    if (!hostId || !confirmation.pluginId) return;
    setIsLoading(true);
    try {
      await hostPluginService.removePluginFromHost(hostId, confirmation.pluginId);
      await fetchAssignedPlugins();
      showNotification('Success', `Plugin "${confirmation.pluginName}" removed successfully`, 'success');
    } catch (error) {
      console.error('Error removing plugin:', error);
      showNotification('Error', 'Failed to remove plugin', 'error');
    } finally {
      setIsLoading(false);
      closeConfirmation();
    }
  };

  const assignedPluginIds = assignedPlugins.map(p => p.id);
  const unassignedPlugins = availablePlugins.filter(plugin => !assignedPluginIds.includes(plugin.id));

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          {host ? `Plugins for ${host.name}` : 'Host Plugins'}
        </h2>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-6">
          {/* Assigned Plugins */}
          <div>
            <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
              Assigned Plugins ({assignedPlugins.length})
            </h3>
            <div className="grid gap-4">
              {assignedPlugins.map(plugin => (
                <div
                  key={plugin.id}
                  className="bg-white dark:bg-gray-800 rounded-lg p-4 shadow border border-gray-200 dark:border-gray-700"
                >
                  <div className="flex justify-between items-start">
                    <div>
                      <h4 className="text-lg font-medium text-gray-900 dark:text-white">
                        {plugin.name}
                      </h4>
                      <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                        {plugin.description}
                      </p>
                    </div>
                    <Button
                      variant="danger"
                      size="sm"
                      onClick={() => showRemoveConfirmation(plugin.id, plugin.name)}
                    >
                      Remove
                    </Button>
                  </div>
                </div>
              ))}
              {assignedPlugins.length === 0 && (
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                  <p className="text-gray-600 dark:text-gray-400">
                    No plugins assigned to this host yet.
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* Available Plugins */}
          <div>
            <h3 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-4">
              Available Plugins ({unassignedPlugins.length})
            </h3>
            <div className="grid gap-4">
              {unassignedPlugins.map(plugin => (
                <div
                  key={plugin.id}
                  className="bg-white dark:bg-gray-800 rounded-lg p-4 shadow border border-gray-200 dark:border-gray-700"
                >
                  <div className="flex justify-between items-start">
                    <div>
                      <h4 className="text-lg font-medium text-gray-900 dark:text-white">
                        {plugin.name}
                      </h4>
                      <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                        {plugin.description}
                      </p>
                    </div>
                    <Button
                      variant="primary"
                      size="sm"
                      onClick={() => handleAddPlugin(plugin.id)}
                    >
                      Add
                    </Button>
                  </div>
                </div>
              ))}
              {unassignedPlugins.length === 0 && (
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
                  <p className="text-gray-600 dark:text-gray-400">
                    No additional plugins available.
                  </p>
                </div>
              )}
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
        onConfirm={handleRemovePlugin}
        title="Remove Plugin"
        message={`Are you sure you want to remove the plugin "${confirmation.pluginName}" from this host?`}
        confirmText="Remove"
        cancelText="Cancel"
        type="danger"
        isLoading={isLoading}
      />
    </div>
  );
};

export default HostPlugins; 