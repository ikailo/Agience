import { useState, useEffect, lazy, Suspense } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import { HostDetailsTab } from '../components/hosts/HostDetailsTab';
import HostList from '../components/hosts/HostList';
import { Host } from '../types/Host';
import { hostService } from '../services/api/hostService';

// Lazy load components to improve performance
const HostKeys = lazy(() => import('../components/hosts/HostKeys'));
const HostPlugins = lazy(() => import('../components/hosts/HostPlugins'));

const tabs = [
  { id: 'details', label: 'Details' },
  { id: 'keys', label: 'Keys' },
  { id: 'plugins', label: 'Plugins' }
];

/**
 * Hosts page component that provides tab navigation for host configuration
 */
const Hosts = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState('details');
  const [hosts, setHosts] = useState<Host[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [selectedHost, setSelectedHost] = useState<Host | null>(null);
  const [hasTempHost, setHasTempHost] = useState(false);

  const hostId = searchParams.get('id');
  const tabParam = searchParams.get('tab') || 'details';

  // Initialize active tab from URL
  useEffect(() => {
    if (tabParam && tabs.some(tab => tab.id === tabParam)) {
      setActiveTab(tabParam);
    }
  }, [tabParam]);

  useEffect(() => {
    const fetchHosts = async () => {
      setIsLoading(true);
      try {
        const fetchedHosts = await hostService.getAllHosts();
        setHosts(fetchedHosts);
        
        // If there's a hostId in the URL, find and select that host
        if (hostId) {
          const host = fetchedHosts.find(h => h.id === hostId);
          if (host) {
            setSelectedHost(host);
          }
        }
      } catch (error) {
        console.error('Error fetching hosts:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchHosts();
  }, [hostId]);

  // Handle tab change
  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    
    // Update URL parameters while preserving the host ID
    const newParams = new URLSearchParams(searchParams);
    newParams.set('tab', tab);
    setSearchParams(newParams);
  };

  // Handle host selection
  const handleSelectHost = (id: string) => {
    const host = hosts.find(h => h.id === id);
    if (host) {
      setSelectedHost(host);
      
      // Update URL parameters
      const newParams = new URLSearchParams(searchParams);
      newParams.set('id', id);
      setSearchParams(newParams);
    }
  };

  // Handle creating a new host
  const handleCreateHost = () => {
    const tempHost: Host = {
      id: `temp-${Date.now()}`,
      name: 'New Host',
      description: '',
      created_date: new Date().toISOString(),
      scopes: []
    };
    
    setHosts(prev => [tempHost, ...prev]);
    setSelectedHost(tempHost);
    setHasTempHost(true);
    
    // Update URL parameters
    const newParams = new URLSearchParams(searchParams);
    newParams.set('id', tempHost.id);
    setSearchParams(newParams);
  };

  // Loading fallback component
  const LoadingFallback = () => (
    <div className="flex justify-center py-12">
      <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
    </div>
  );

  // Empty state component when no host is selected
  const NoHostSelected = () => (
    <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center shadow-lg">
      <p className="text-gray-600 dark:text-gray-300">Please select a host from the list first.</p>
    </div>
  );

  return (
    <div className="space-y-6">
      <TabNavigation
        tabs={tabs}
        activeTab={activeTab}
        onTabChange={handleTabChange}
      />

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Host List - Left Side */}
        <div className="lg:col-span-1">
          <HostList
            hosts={hosts}
            selectedHostId={selectedHost?.id || null}
            isLoading={isLoading}
            onSelectHost={handleSelectHost}
            onCreateHost={handleCreateHost}
            hasTempHost={hasTempHost}
          />
        </div>

        {/* Tab Content - Right Side */}
        <div className="lg:col-span-3">
          <Suspense fallback={<LoadingFallback />}>
            {activeTab === 'details' && (
              <HostDetailsTab />
            )}
            
            {activeTab === 'keys' && (
              selectedHost ? (
                <HostKeys hostId={selectedHost.id} />
              ) : (
                <NoHostSelected />
              )
            )}
            
            {activeTab === 'plugins' && (
              selectedHost ? (
                <HostPlugins hostId={selectedHost.id} />
              ) : (
                <NoHostSelected />
              )
            )}
          </Suspense>
        </div>
      </div>
    </div>
  );
};

export default Hosts;
