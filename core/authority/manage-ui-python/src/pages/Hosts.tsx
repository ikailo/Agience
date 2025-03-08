import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import HostKeys from '../components/hosts/HostKeys';
import { HostPlugins } from '../components/hosts/HostPlugins';
import { HostDetailsTab } from '../components/hosts/HostDetailsTab';

/**
 * Hosts page component that provides tab navigation for host configuration
 * Supports both light and dark modes
 */
const Hosts = () => {
  const [activeTab, setActiveTab] = useState('details');
  const [searchParams] = useSearchParams();
  const hostId = searchParams.get('id');
  
  // Log when the Hosts page renders
  useEffect(() => {
    console.log('Hosts page rendered, active tab:', activeTab);
    console.log('URL search params:', Object.fromEntries(searchParams.entries()));
    
    // Log when the Keys tab is active
    if (activeTab === 'keys') {
      console.log('Keys tab is active with hostId:', hostId);
    }
  }, [activeTab, searchParams, hostId]);

  // Handle tab change
  const handleTabChange = (tab: string) => {
    console.log(`Tab change requested from ${activeTab} to ${tab}`);
    setActiveTab(tab);
    
    // Preserve the host ID when switching tabs
    if (hostId) {
      console.log(`Switching to ${tab} tab with host ID: ${hostId}`);
    } else {
      console.log(`Switching to ${tab} tab with no host ID`);
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Host Configuration</h1>

      {/* Tab Navigation */}
      <div className="mb-4 border-b border-gray-200 dark:border-gray-700">
        <ul className="flex flex-wrap -mb-px">
          <li className="mr-2">
            <button
              onClick={() => handleTabChange('details')}
              data-tab="details"
              className={`inline-block p-4 border-b-2 rounded-t-lg ${
                activeTab === 'details'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
              }`}
            >
              Details
            </button>
          </li>
          <li className="mr-2">
            <button
              onClick={() => handleTabChange('keys')}
              data-tab="keys"
              className={`inline-block p-4 border-b-2 rounded-t-lg ${
                activeTab === 'keys'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
              }`}
            >
              Keys
            </button>
          </li>
          <li className="mr-2">
            <button
              onClick={() => handleTabChange('plugins')}
              data-tab="plugins"
              className={`inline-block p-4 border-b-2 rounded-t-lg ${
                activeTab === 'plugins'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
              }`}
            >
              Plugins
            </button>
          </li>
        </ul>
      </div>

      {/* Tab Content */}
      <div className="mt-4">
        {activeTab === 'details' && <HostDetailsTab />}
        {activeTab === 'keys' && (
          <HostKeys hostId={hostId || undefined} />
        )}
        {activeTab === 'plugins' && <HostPlugins />}
      </div>
    </div>
  );
};

export default Hosts;
