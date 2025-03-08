import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TopicDetails } from '../components/topics/TopicDetails';
import TopicAgents from '../components/topics/TopicAgents';

/**
 * Topics page component that provides tab navigation for topic configuration
 * Supports both light and dark modes
 */
const Topics = () => {
  const [activeTab, setActiveTab] = useState('details');
  const [searchParams, setSearchParams] = useSearchParams();
  const topicId = searchParams.get('id');
  
  // Log when the Topics page renders
  useEffect(() => {
    console.log('Topics page rendered, active tab:', activeTab);
    console.log('URL search params:', Object.fromEntries(searchParams.entries()));
  }, [activeTab, searchParams]);

  // Handle tab change
  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    // Preserve the topic ID when switching tabs
    if (topicId) {
      console.log(`Switching to ${tab} tab with topic ID: ${topicId}`);
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Topic Configuration</h1>

      {/* Tab Navigation */}
      <div className="mb-4 border-b border-gray-200 dark:border-gray-700">
        <ul className="flex flex-wrap -mb-px">
          <li className="mr-2">
            <button
              onClick={() => handleTabChange('details')}
              data-tab="details"
              className={`inline-block p-4 border-b-2 rounded-t-lg ${activeTab === 'details'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
                }`}
            >
              Details
            </button>
          </li>
          <li className="mr-2">
            <button
              onClick={() => handleTabChange('agents')}
              data-tab="agents"
              className={`inline-block p-4 border-b-2 rounded-t-lg ${activeTab === 'agents'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
                }`}
            >
              Agents
            </button>
          </li>
        </ul>
      </div>

      {/* Tab Content */}
      <div className="mt-4">
        {activeTab === 'details' ? <TopicDetails /> : <TopicAgents />}
      </div>
    </div>
  );
};

export default Topics;
