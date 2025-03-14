import React from 'react';
import { Connection } from '../../types/Connection';

interface ConnectionDetailsProps {
  connection: Connection;
  onEdit: () => void;
  onDelete: () => void;
}

/**
 * Component for displaying connection details
 */
const ConnectionDetails: React.FC<ConnectionDetailsProps> = ({
  connection,
  onEdit,
  onDelete
}) => {
  return (
    <div className="bg-gray-800 rounded-lg p-6">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-white">
          Connection Details
        </h2>
        <div className="flex space-x-3">
          <button
            onClick={onEdit}
            className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors"
          >
            Edit
          </button>
          <button
            onClick={onDelete}
            className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
          >
            Delete
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <h3 className="text-sm font-medium text-gray-400 mb-1">Name</h3>
          <p className="text-white text-lg">{connection.name}</p>
        </div>
        <div>
          <h3 className="text-sm font-medium text-gray-400 mb-1">Authorizer</h3>
          <div className="flex items-center">
            <span className="px-3 py-1 inline-flex text-sm leading-5 font-semibold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300">
              {connection.authorizer?.name || 'Unknown'}
            </span>
            {connection.authorizer && (
              <span className="ml-2 text-gray-400 text-sm">
                ({connection.authorizer.type})
              </span>
            )}
          </div>
        </div>
        <div className="md:col-span-2">
          <h3 className="text-sm font-medium text-gray-400 mb-1">Description</h3>
          <p className="text-white">{connection.description}</p>
        </div>
        {connection.created_date && (
          <div>
            <h3 className="text-sm font-medium text-gray-400 mb-1">Created Date</h3>
            <p className="text-white">
              {new Date(connection.created_date).toLocaleDateString()}
            </p>
          </div>
        )}
        <div>
          <h3 className="text-sm font-medium text-gray-400 mb-1">ID</h3>
          <p className="text-white text-sm font-mono bg-gray-700 p-2 rounded">{connection.id}</p>
        </div>
        {connection.authorizer && (
          <div className="md:col-span-2 p-4 bg-gray-700 rounded-lg">
            <h3 className="text-sm font-medium text-gray-300 mb-2">Authorizer Details</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <h4 className="text-xs font-medium text-gray-400">Type</h4>
                <p className="text-white">{connection.authorizer.type}</p>
              </div>
              <div>
                <h4 className="text-xs font-medium text-gray-400">ID</h4>
                <p className="text-white text-sm font-mono">{connection.authorizer.id}</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ConnectionDetails; 