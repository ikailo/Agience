import React from 'react';
import { Topic } from '../../../types/Topic';

interface TopicCardProps {
  topic: Topic;
  isAssigned: boolean;
  onToggle: (topicId: string) => void;
}

/**
 * TopicCard component that displays an individual topic
 */
const TopicCard: React.FC<TopicCardProps> = ({ topic, isAssigned, onToggle }) => {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-3 sm:p-4 border border-gray-200 dark:border-gray-700 h-full flex flex-col">
      <div className="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-2 h-full">
        <div className="flex-1">
          <h3 className="text-base sm:text-lg font-medium text-gray-900 dark:text-white line-clamp-2">{topic.name}</h3>
          <p className="text-sm text-gray-600 dark:text-gray-400 mt-1 line-clamp-3">{topic.description}</p>
          {topic.created_date && (
            <p className="text-xs text-gray-500 dark:text-gray-500 mt-2">
              Created: {new Date(topic.created_date).toLocaleDateString()}
            </p>
          )}
        </div>
        <div className="mt-2 sm:mt-0">
          <button
            onClick={() => onToggle(topic.id)}
            className={`px-3 py-1 rounded-md text-sm font-medium transition-colors whitespace-nowrap ${
              isAssigned
                ? 'bg-red-100 text-red-700 hover:bg-red-200 dark:bg-red-900 dark:text-red-300 dark:hover:bg-red-800'
                : 'bg-green-100 text-green-700 hover:bg-green-200 dark:bg-green-900 dark:text-green-300 dark:hover:bg-green-800'
            }`}
          >
            {isAssigned ? 'Remove' : 'Add'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default TopicCard; 