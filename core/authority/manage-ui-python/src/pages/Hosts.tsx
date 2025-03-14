import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import HostKeys from '../components/hosts/HostKeys';
import { HostPlugins } from '../components/hosts/HostPlugins';
import { HostDetailsTab } from '../components/hosts/HostDetailsTab';
import { TabNavigation } from '../components/common/TabNavigation';
import Card from '../components/common/Card';
import HostList from '../components/hosts/HostList';
import { Host } from '../types/Host';
import { hostService } from '../services/api/hostService';

const tabs = [
  { id: 'details', label: 'Details' },
  { id: 'keys', label: 'Keys' },
  { id: 'plugins', label: 'Plugins' }
];

/**
 * Hosts page component that provides tab navigation for host configuration
 * Supports both light and dark modes
 */
const Hosts = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState('details');
  const [hosts, setHosts] = useState<Host[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [selectedHost, setSelectedHost] = useState<Host | null>(null);
  const [hasTempHost, setHasTempHost] = useState(false);

  const hostId = searchParams.get('id');

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
          {activeTab === 'details' && (
            <HostDetailsTab />
          )}
          {activeTab === 'keys' && selectedHost && (
            <HostKeys hostId={selectedHost.id} />
          )}
          {activeTab === 'plugins' && selectedHost && (
            <HostPlugins hostId={selectedHost.id} />
          )}
        
        </div>
      </div>
    </div>
  );
};

export default Hosts;
