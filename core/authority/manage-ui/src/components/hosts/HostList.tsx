import React from 'react';
import { Host } from '../../types/Host';
import Card from '../common/Card';

interface HostListProps {
  hosts: Host[];
  selectedHostId: string | null;
  isLoading: boolean;
  onSelectHost: (id: string) => void;
  onCreateHost: () => void;
  hasTempHost?: boolean;
}

/**
 * HostList component that displays a list of hosts
 */
const HostList: React.FC<HostListProps> = ({
  hosts,
  selectedHostId,
  isLoading,
  onSelectHost,
  onCreateHost,
  hasTempHost = false
}) => {
  return (
    <Card
      title="My Hosts"
      actions={
        <button
          onClick={onCreateHost}
          className={`px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors flex items-center space-x-1 text-sm font-medium shadow-sm ${
            hasTempHost ? 'opacity-50 cursor-not-allowed' : ''
          }`}
          disabled={hasTempHost}
          title={hasTempHost ? "Save or cancel the current new host first" : "Create a new host"}
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          <span>New Host</span>
        </button>
      }
    >
      <div className="space-y-3 max-h-[calc(100vh-250px)] overflow-y-auto pr-1">
        {isLoading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
          </div>
        ) : hosts.length === 0 ? (
          <div className="text-center py-8 px-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <svg className="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M5 12h14M12 5l7 7-7 7" />
            </svg>
            <p className="mt-2 text-gray-600 dark:text-gray-400">No hosts found</p>
            <button
              onClick={onCreateHost}
              className={`mt-3 px-4 py-2 bg-indigo-600 text-white text-sm rounded-md hover:bg-indigo-700 transition-colors ${
                hasTempHost ? 'opacity-50 cursor-not-allowed' : ''
              }`}
              disabled={hasTempHost}
            >
              Create your first host
            </button>
          </div>
        ) : (
          hosts.map(host => (
            <div
              key={host.id}
              className={`
                p-3 rounded-lg cursor-pointer transition-colors
                ${selectedHostId === host.id
                  ? 'bg-indigo-50 dark:bg-indigo-900 border-l-4 border-indigo-500'
                  : 'bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 border-l-4 border-transparent'
                }
                ${host.id.startsWith('temp-') ? 'border border-dashed border-indigo-500 dark:border-indigo-400' : ''}
              `}
              onClick={() => onSelectHost(host.id)}
            >
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-medium text-gray-900 dark:text-white flex items-center">
                    {host.name}
                    {host.id.startsWith('temp-') && (
                      <span className="ml-2 px-2 py-0.5 text-xs bg-indigo-100 text-indigo-800 dark:bg-indigo-800 dark:text-indigo-200 rounded-full">
                        New
                      </span>
                    )}
                  </h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400 mt-1 line-clamp-2">
                    {host.description || 'No description'}
                  </p>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </Card>
  );
};

export default HostList;