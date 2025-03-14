import React from 'react';
import { Authorizer } from '../../types/Authorizer';

interface AuthorizerDetailsProps {
  authorizer: Authorizer;
  onEdit: () => void;
  onDelete: () => void;
}

/**
 * Component for displaying authorizer details
 */
const AuthorizerDetails: React.FC<AuthorizerDetailsProps> = ({
  authorizer,
  onEdit,
  onDelete
}) => {
  return (
    <div className="bg-gray-800 rounded-lg p-6">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-white">
          Authorizer Details
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
          <p className="text-white text-lg">{authorizer.name}</p>
        </div>
        <div>
          <h3 className="text-sm font-medium text-gray-400 mb-1">Type</h3>
          <span className="px-3 py-1 inline-flex text-sm leading-5 font-semibold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300">
            {authorizer.type}
          </span>
        </div>
        {authorizer.created_date && (
          <div>
            <h3 className="text-sm font-medium text-gray-400 mb-1">Created Date</h3>
            <p className="text-white">
              {new Date(authorizer.created_date).toLocaleDateString()}
            </p>
          </div>
        )}
        <div>
          <h3 className="text-sm font-medium text-gray-400 mb-1">ID</h3>
          <p className="text-white text-sm font-mono bg-gray-700 p-2 rounded">{authorizer.id}</p>
        </div>
      </div>
    </div>
  );
};

export default AuthorizerDetails; 