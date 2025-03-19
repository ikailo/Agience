import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TabNavigation } from '../components/common/TabNavigation';
import { TopicDetails } from '../components/topics/TopicDetails';
import TopicAgentsTab from '../components/topics/TopicAgentsTab';

const tabs = [
  { id: 'details', label: 'Details' },
  { id: 'agents', label: 'Agents' },
];

/**
 * Topic page component that provides tab navigation for topic configuration
 */
export default function Topic() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState('details');
  
  // Get topic ID from URL if available
  const topicId = searchParams.get('id');
  
  // Initialize state from URL parameters
  useEffect(() => {
    const tab = searchParams.get('tab');
    if (tab) {
      setActiveTab(tab);
    }
  }, [searchParams]);

  // Handle tab change
  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    
    // Update URL parameters while preserving the topic ID
    const newParams = new URLSearchParams(searchParams);
    newParams.set('tab', tab);
    setSearchParams(newParams);
  };

  return (
    <div className="space-y-6">
      <TabNavigation
        tabs={tabs}
        activeTab={activeTab}
        onTabChange={handleTabChange}
      />

      <div className="mt-6">
        {activeTab === 'details' && <TopicDetails />}
        {activeTab === 'agents' && <TopicAgentsTab topicId={topicId || undefined} />}
      </div>
    </div>
  );
} 