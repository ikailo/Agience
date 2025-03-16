import React from 'react';
import { Agent } from '../../types/Agent';
import Card from '../common/Card';

interface AgentListProps {
  agents: Agent[];
  selectedAgentId: string | null;
  isLoading: boolean;
  onSelectAgent: (id: string) => void;
  onCreateAgent: () => void;
  hasTempAgent?: boolean;
}

/**
 * AgentList component that displays a list of agents
 */
const AgentList: React.FC<AgentListProps> = ({
  agents,
  selectedAgentId,
  isLoading,
  onSelectAgent,
  onCreateAgent,
  hasTempAgent = false
}) => {
  return (
    <Card
      title="My Agents"
      actions={
        <button
          onClick={onCreateAgent}
          className={`px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors flex items-center space-x-1 text-sm font-medium shadow-sm ${
            hasTempAgent ? 'opacity-50 cursor-not-allowed' : ''
          }`}
          disabled={hasTempAgent}
          title={hasTempAgent ? "Save or cancel the current new agent first" : "Create a new agent"}
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          <span>New Agent</span>
        </button>
      }
    >
      <div className="space-y-3 max-h-[calc(100vh-250px)] overflow-y-auto pr-1">
        {isLoading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
          </div>
        ) : agents.length === 0 ? (
          <div className="text-center py-8 px-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <svg className="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.75 3.104v5.714a2.25 2.25 0 01-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 014.5 0m0 0v5.714a2.25 2.25 0 001.5 2.25m0 0v5.8a2.25 2.25 0 01-2.25 2.25H6.75a2.25 2.25 0 01-2.25-2.25V8.25a2.25 2.25 0 011.5-2.25m7.5 0a15.645 15.645 0 013-1.3m-3 1.3a15.65 15.65 0 00-3 1.3m0 0h3" />
            </svg>
            <p className="mt-2 text-gray-600 dark:text-gray-400">No agents found</p>
            <button
              onClick={onCreateAgent}
              className={`mt-3 px-4 py-2 bg-indigo-600 text-white text-sm rounded-md hover:bg-indigo-700 transition-colors ${
                hasTempAgent ? 'opacity-50 cursor-not-allowed' : ''
              }`}
              disabled={hasTempAgent}
            >
              Create your first agent
            </button>
          </div>
        ) : (
          agents.map(agent => (
            <div
              key={agent.id}
              className={`
                p-3 rounded-lg cursor-pointer transition-colors
                ${selectedAgentId === agent.id
                  ? 'bg-indigo-50 dark:bg-indigo-900 border-l-4 border-indigo-500'
                  : 'bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 border-l-4 border-transparent'
                }
                ${agent.id.startsWith('temp-') ? 'border border-dashed border-indigo-500 dark:border-indigo-400' : ''}
              `}
              onClick={() => onSelectAgent(agent.id)}
            >
              <div className="flex flex-col">
                <h3 className="font-medium text-gray-900 dark:text-white flex items-center">
                  {agent.name}
                  {agent.id.startsWith('temp-') && (
                    <span className="ml-2 px-2 py-0.5 text-xs bg-indigo-100 text-indigo-800 dark:bg-indigo-800 dark:text-indigo-200 rounded-full">
                      New
                    </span>
                  )}
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 truncate">
                  {agent.description || 'No description'}
                </p>
                <div className="flex items-center mt-1">
                  <div className={`h-2 w-2 rounded-full ${agent.is_enabled ? 'bg-green-500' : 'bg-gray-300 dark:bg-gray-600'}`} />
                  <span className="text-xs text-gray-500 dark:text-gray-400 ml-1">
                    {agent.is_enabled ? 'Enabled' : 'Disabled'}
                  </span>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </Card>
  );
};

export default AgentList; 