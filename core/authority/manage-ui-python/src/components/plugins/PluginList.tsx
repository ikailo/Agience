import React from 'react';
import { Plugin } from '../../types/Plugin';
import Card from '../common/Card';

interface PluginListProps {
  plugins: Plugin[];
  selectedPluginId: string | null;
  isLoading: boolean;
  onSelectPlugin: (id: string) => void;
  onCreatePlugin: () => void;
  hasTempPlugin?: boolean;
}

/**
 * PluginList component that displays a list of plugins
 */
const PluginList: React.FC<PluginListProps> = ({
  plugins,
  selectedPluginId,
  isLoading,
  onSelectPlugin,
  onCreatePlugin,
  hasTempPlugin = false
}) => {
  return (
    <Card
      title="My Plugins"
      actions={
        <button
          onClick={onCreatePlugin}
          className={`px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors flex items-center space-x-1 text-sm font-medium shadow-sm ${
            hasTempPlugin ? 'opacity-50 cursor-not-allowed' : ''
          }`}
          disabled={hasTempPlugin}
          title={hasTempPlugin ? "Save or cancel the current new plugin first" : "Create a new plugin"}
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          <span>New Plugin</span>
        </button>
      }
    >
      <div className="space-y-3 max-h-[calc(100vh-250px)] overflow-y-auto pr-1">
        {isLoading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
          </div>
        ) : plugins.length === 0 ? (
          <div className="text-center py-8 px-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <svg className="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            <p className="mt-2 text-gray-600 dark:text-gray-400">No plugins found</p>
            <button
              onClick={onCreatePlugin}
              className={`mt-3 px-4 py-2 bg-indigo-600 text-white text-sm rounded-md hover:bg-indigo-700 transition-colors ${
                hasTempPlugin ? 'opacity-50 cursor-not-allowed' : ''
              }`}
              disabled={hasTempPlugin}
            >
              Create your first plugin
            </button>
          </div>
        ) : (
          plugins.map(plugin => (
            <div
              key={plugin.id}
              className={`
                p-3 rounded-lg cursor-pointer transition-colors
                ${selectedPluginId === plugin.id
                  ? 'bg-indigo-50 dark:bg-indigo-900 border-l-4 border-indigo-500'
                  : 'bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 border-l-4 border-transparent'
                }
                ${plugin.id.startsWith('temp-') ? 'border border-dashed border-indigo-500 dark:border-indigo-400' : ''}
              `}
              onClick={() => onSelectPlugin(plugin.id)}
            >
              <div className="flex flex-col">
                <h3 className="font-medium text-gray-900 dark:text-white flex items-center">
                  {plugin.name}
                  {plugin.id.startsWith('temp-') && (
                    <span className="ml-2 px-2 py-0.5 text-xs bg-indigo-100 text-indigo-800 dark:bg-indigo-800 dark:text-indigo-200 rounded-full">
                      New
                    </span>
                  )}
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 font-mono truncate">
                  {plugin.id}
                </p>
              </div>
            </div>
          ))
        )}
      </div>
    </Card>
  );
};

export default PluginList; 