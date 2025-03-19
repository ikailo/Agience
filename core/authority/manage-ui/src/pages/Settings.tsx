import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import AuthorizersTab from '../components/settings/AuthorizersTab';
import ConnectionsTab from '../components/settings/ConnectionsTab';

/**
 * Settings page component with tab navigation
 */
const Settings = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState<string>('authorizers');
  const [hasAuthorizers, setHasAuthorizers] = useState<boolean>(false);

  // Initialize state from URL parameters
  useEffect(() => {
    const tab = searchParams.get('tab');
    if (tab) {
      setActiveTab(tab);
    }
  }, [searchParams]);

  /**
   * Handles tab change
   */
  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    
    // Update URL parameters
    const newParams = new URLSearchParams(searchParams);
    newParams.set('tab', tab);
    setSearchParams(newParams);
  };

  /**
   * Callback for when authorizers are updated
   */
  const handleAuthorizersChange = useCallback((hasAny: boolean) => {
    setHasAuthorizers(hasAny);
  }, []);

  // Define tabs
  const tabs = [
    { id: 'authorizers', label: 'Authorizers' },
    { id: 'connections', label: 'Connections', disabled: !hasAuthorizers }
  ];

  return (
    <div className="flex flex-col h-full">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white mb-6">
        Settings & Privacy
      </h1>
      
      {/* Tabs at the top */}
      <div className="mb-6">
        <TabNavigation
          tabs={tabs}
          activeTab={activeTab}
          onTabChange={handleTabChange}
        />
      </div>
      
      {/* Content area */}
      <div className="flex-grow">
        {activeTab === 'authorizers' && (
          <AuthorizersTab onAuthorizersChange={handleAuthorizersChange} />
        )}
        
        {activeTab === 'connections' && hasAuthorizers && (
          <ConnectionsTab />
        )}
        
        {activeTab === 'connections' && !hasAuthorizers && (
          <div className="flex flex-col items-center justify-center h-64 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <svg className="h-16 w-16 text-gray-400 dark:text-gray-500 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
            <p className="text-gray-600 dark:text-gray-400 mb-4">You need to create at least one authorizer first</p>
            <button
              onClick={() => handleTabChange('authorizers')}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
            >
              Go to Authorizers
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default Settings; 