import React from 'react';
import { Plugin } from '../../../types/Plugin';

interface PluginCardProps {
  plugin: Plugin;
  isAssigned: boolean;
  onToggle: (pluginId: string) => void;
}

/**
 * PluginCard component that displays an individual plugin with toggle functionality
 */
const PluginCard: React.FC<PluginCardProps> = ({ plugin, isAssigned, onToggle }) => {
  const getProviderBadgeColor = (provider: string) => {
    switch (provider) {
      case 'Collection':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300';
      case 'Prompts':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300';
      case 'Semantic Kernel Plugin':
        return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
    }
  };

  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-3 sm:p-4 border border-gray-200 dark:border-gray-700 h-full flex flex-col">
      <div className="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-2 h-full">
        <div className="flex-1">
          <h3 className="text-base sm:text-lg font-medium text-gray-900 dark:text-white line-clamp-2">{plugin.name}</h3>
          <p className="text-sm text-gray-600 dark:text-gray-400 mt-1 line-clamp-3">{plugin.description}</p>
          <div className="mt-2">
            <span className={`inline-block px-2 py-1 text-xs font-medium rounded-full ${getProviderBadgeColor(plugin.provider)}`}>
              {plugin.provider}
            </span>
          </div>
        </div>
        <div className="mt-2 sm:mt-0">
          <button
            onClick={() => onToggle(plugin.id)}
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

export default PluginCard; 