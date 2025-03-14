import React from 'react';
import { Agent } from '../../types/Agent';

interface AgentCardProps {
  agent: Agent;
  isSelected: boolean;
  onSelect: (id: string) => void;
  isTemp?: boolean;
}

/**
 * AgentCard component that displays an individual agent
 */
const AgentCard: React.FC<AgentCardProps> = ({ agent, isSelected, onSelect, isTemp = false }) => {
  return (
    <div
      className={`
        p-3 rounded-lg cursor-pointer transition-colors
        ${isSelected
          ? 'bg-indigo-50 dark:bg-indigo-900 border-l-4 border-indigo-500'
          : 'bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 border-l-4 border-transparent'
        }
        ${isTemp ? 'border border-dashed border-indigo-500 dark:border-indigo-400' : ''}
      `}
      onClick={() => onSelect(agent.id)}
    >
      <div className="flex items-center justify-between">
        <div>
          <h3 className="font-medium text-gray-900 dark:text-white flex items-center">
            {agent.name}
            {isTemp && (
              <span className="ml-2 px-2 py-0.5 text-xs bg-indigo-100 text-indigo-800 dark:bg-indigo-800 dark:text-indigo-200 rounded-full">
                New
              </span>
            )}
          </h3>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1 line-clamp-2">
            {agent.description || 'No description'}
          </p> 
        </div>
        <div className={`h-3 w-3 rounded-full ${agent.is_enabled ? 'bg-green-500' : 'bg-gray-300 dark:bg-gray-600'}`} />
      </div>
    </div>
  );
};

export default AgentCard; 