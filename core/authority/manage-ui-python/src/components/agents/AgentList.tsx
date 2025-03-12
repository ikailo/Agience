import React from 'react';
import { Agent } from '../../types/Agent';
import AgentCard from './AgentCard';

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
    <div className="bg-white dark:bg-gray-900 p-4 rounded-lg shadow-lg h-full">
      <div className="flex flex-wrap justify-between items-center mb-4 gap-2">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white">My Agents</h2>
        <button
          onClick={onCreateAgent}
          className={`px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors flex items-center space-x-1 text-sm font-medium shadow-sm ${hasTempAgent ? 'opacity-50 cursor-not-allowed' : ''}`}
          disabled={hasTempAgent}
          title={hasTempAgent ? "Save or cancel the current new agent first" : "Create a new agent"}
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          <span>New Agent</span>
        </button>
      </div>
      
      {/* Search box - Uncomment if needed */}
      {/* <div className="relative mb-4">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <svg className="h-5 w-5 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <input
          type="text"
          placeholder="Search agents..."
          className="w-full pl-10 pr-4 py-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-md text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
        />
      </div> */}
      
      {isLoading && agents.length === 0 ? (
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
        </div>
      ) : (
        <div className="space-y-3 max-h-[calc(100vh-250px)] overflow-y-auto pr-1 sm:max-h-[500px]">
          {agents.length === 0 ? (
            <div className="text-center py-8 px-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
              <svg className="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.75 3.104v5.714a2.25 2.25 0 01-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 014.5 0m0 0v5.714a2.25 2.25 0 001.5 2.25m0 0v5.8a2.25 2.25 0 01-2.25 2.25H6.75a2.25 2.25 0 01-2.25-2.25V8.25a2.25 2.25 0 011.5-2.25m7.5 0a15.645 15.645 0 013-1.3m-3 1.3a15.65 15.65 0 00-3 1.3m0 0h3" />
              </svg>
              <p className="mt-2 text-gray-600 dark:text-gray-400">No agents found</p>
              <button
                onClick={onCreateAgent}
                className={`mt-3 px-4 py-2 bg-indigo-600 text-white text-sm rounded-md hover:bg-indigo-700 transition-colors ${hasTempAgent ? 'opacity-50 cursor-not-allowed' : ''}`}
                disabled={hasTempAgent}
              >
                Create your first agent
              </button>
            </div>
          ) : (
            agents.map(agent => (
              <AgentCard
                key={agent.id}
                agent={agent}
                isSelected={agent.id === selectedAgentId}
                onSelect={onSelectAgent}
                isTemp={agent.id.startsWith('temp-')}
              />
            ))
          )}
        </div>
      )}
    </div>
  );
};

export default AgentList; 