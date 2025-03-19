import React from 'react';
import { Authorizer } from '../../types/Authorizer';
import Card from '../common/Card';

interface AuthorizerListProps {
  authorizers: Authorizer[];
  selectedAuthorizerId: string | null;
  isLoading: boolean;
  onSelectAuthorizer: (id: string) => void;
  onCreateAuthorizer: () => void;
  hasTempAuthorizer?: boolean;
}

/**
 * AuthorizerList component that displays a list of authorizers
 */
const AuthorizerList: React.FC<AuthorizerListProps> = ({
  authorizers,
  selectedAuthorizerId,
  isLoading,
  onSelectAuthorizer,
  onCreateAuthorizer,
  hasTempAuthorizer = false
}) => {
  return (
    <Card
      title="My Authorizers"
      actions={
        <button
          onClick={onCreateAuthorizer}
          className={`px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors flex items-center space-x-1 text-sm font-medium shadow-sm ${
            hasTempAuthorizer ? 'opacity-50 cursor-not-allowed' : ''
          }`}
          disabled={hasTempAuthorizer}
          title={hasTempAuthorizer ? "Save or cancel the current new authorizer first" : "Create a new authorizer"}
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          <span>New Authorizer</span>
        </button>
      }
    >
      <div className="space-y-3 max-h-[calc(100vh-250px)] overflow-y-auto pr-1">
        {isLoading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
          </div>
        ) : authorizers.length === 0 ? (
          <div className="text-center py-8 px-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <svg className="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
            <p className="mt-2 text-gray-600 dark:text-gray-400">No authorizers found</p>
            <button
              onClick={onCreateAuthorizer}
              className={`mt-3 px-4 py-2 bg-indigo-600 text-white text-sm rounded-md hover:bg-indigo-700 transition-colors ${
                hasTempAuthorizer ? 'opacity-50 cursor-not-allowed' : ''
              }`}
              disabled={hasTempAuthorizer}
            >
              Create your first authorizer
            </button>
          </div>
        ) : (
          authorizers.map(authorizer => (
            <div
              key={authorizer.id}
              className={`
                p-3 rounded-lg cursor-pointer transition-colors
                ${selectedAuthorizerId === authorizer.id
                  ? 'bg-indigo-50 dark:bg-indigo-900 border-l-4 border-indigo-500'
                  : 'bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 border-l-4 border-transparent'
                }
                ${authorizer.id.startsWith('temp-') ? 'border border-dashed border-indigo-500 dark:border-indigo-400' : ''}
              `}
              onClick={() => onSelectAuthorizer(authorizer.id)}
            >
              <div className="flex flex-col">
                <h3 className="font-medium text-gray-900 dark:text-white flex items-center">
                  {authorizer.name}
                  {authorizer.id.startsWith('temp-') && (
                    <span className="ml-2 px-2 py-0.5 text-xs bg-indigo-100 text-indigo-800 dark:bg-indigo-800 dark:text-indigo-200 rounded-full">
                      New
                    </span>
                  )}
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  {authorizer.type}
                </p>
              </div>
            </div>
          ))
        )}
      </div>
    </Card>
  );
};

export default AuthorizerList; 