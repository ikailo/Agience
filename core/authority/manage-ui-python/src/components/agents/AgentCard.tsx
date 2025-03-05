import React from 'react';
import { Agent } from '../../types/Agent';

interface AgentCardProps {
  agent: Agent;
  isSelected: boolean;
  onSelect: (id: string) => void;
}

/**
 * AgentCard component that displays an individual agent in the list with avatar
 */
const AgentCard: React.FC<AgentCardProps> = ({ agent, isSelected, onSelect }) => {
  return (
    <div
      onClick={() => onSelect(agent.id)}
      className={`p-3 rounded cursor-pointer transition-all duration-200 hover:shadow-md ${
        isSelected
          ? 'bg-indigo-700 text-white border-l-4 border-indigo-400 dark:bg-indigo-700 dark:text-white dark:border-indigo-400'
          : 'bg-gray-100 text-gray-800 hover:bg-gray-200 border-l-4 border-transparent dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700 dark:border-transparent'
      }`}
    >
      <div className="flex items-center space-x-3">
        <div className="flex-shrink-0">
          <img 
            src={agent.imageUrl || "/astra-avatar.png"} 
            alt={`${agent.name} avatar`}
            className={`w-10 h-10 rounded-full object-cover border-2 ${
              isSelected 
                ? 'border-indigo-300' 
                : 'border-gray-300 dark:border-gray-600'
            }`}
          />
        </div>
        <div className="flex-1 min-w-0">
          <div className="font-medium text-base truncate">{agent.name}</div>
          <div className={`text-sm truncate ${
            isSelected 
              ? 'text-indigo-200' 
              : 'text-gray-600 dark:text-gray-400'
          }`}>
            {agent.description}
          </div>
          <div className="flex items-center mt-1">
            <span 
              className={`inline-block w-2 h-2 rounded-full mr-2 ${
                agent.is_enabled 
                  ? 'bg-green-500' 
                  : 'bg-gray-400 dark:bg-gray-500'
              }`}
            ></span>
            <span className={`text-xs ${
              isSelected 
                ? 'text-indigo-200' 
                : 'text-gray-500 dark:text-gray-500'
            }`}>
              {agent.is_enabled ? 'Enabled' : 'Disabled'}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AgentCard; 