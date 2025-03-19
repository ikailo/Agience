import React from 'react';
import { Topic } from '../../types/Topic';
import { useSearchParams } from 'react-router-dom';

interface TopicTableProps {
  topics: Topic[];
  onEdit: (topic: Topic) => void;
  onDelete: (topic: Topic) => void;
  isLoading: boolean;
}

/**
 * TopicTable component that displays a list of topics in a table
 * Supports both light and dark modes and is mobile responsive
 */
const TopicTable: React.FC<TopicTableProps> = ({ topics, onEdit, onDelete, isLoading }) => {
  const [searchParams, setSearchParams] = useSearchParams();
  const selectedTopicId = searchParams.get('id');

  // Function to handle topic selection
  const handleSelectTopic = (topic: Topic) => {
    // console.log('topic:', topic.id);
    setSearchParams({ id: topic.id });
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-500"></div>
      </div>
    );
  }

  if (topics.length === 0) {
    return (
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center border border-gray-200 dark:border-gray-700">
        <p className="text-gray-600 dark:text-gray-400">
          No topics found. Create your first topic to get started.
        </p>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Name
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Description
              </th>
              <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            {topics.map((topic) => (
              <tr 
                key={topic.id} 
                className={`hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer ${
                  selectedTopicId === topic.id ? 'bg-indigo-50 dark:bg-indigo-900/20' : ''
                }`}
                onClick={() => handleSelectTopic(topic)}
              >
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900 dark:text-white">
                    {topic.name}
                  </div>
                </td>
                <td className="px-6 py-4">
                  <div className="text-sm text-gray-500 dark:text-gray-400 line-clamp-2">
                    {topic.description || 'No description'}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <button
                    onClick={(e) => {
                      e.stopPropagation(); // Prevent row click
                      handleSelectTopic(topic);
                      // Switch to the Agents tab
                      const agentsTabButton = document.querySelector('button[data-tab="agents"]');
                      if (agentsTabButton) {
                        (agentsTabButton as HTMLButtonElement).click();
                      }
                    }}
                    className="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300 transition-colors mr-4"
                  >
                    View Agents
                  </button>
                  <button
                    onClick={(e) => {
                      e.stopPropagation(); // Prevent row click
                      onEdit(topic);
                    }}
                    className="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 transition-colors mr-4"
                  >
                    Edit
                  </button>
                  <button
                    onClick={(e) => {
                      e.stopPropagation(); // Prevent row click
                      onDelete(topic);
                    }}
                    className="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300 transition-colors"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default TopicTable; 