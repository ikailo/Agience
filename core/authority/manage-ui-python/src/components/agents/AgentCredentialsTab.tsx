import React from 'react';

interface AgentCredentialsTabProps {
  agentId?: string;
}

/**
 * Component for managing agent credentials
 */
function AgentCredentialsTab({ agentId }: AgentCredentialsTabProps) {
  return (
    <div className="p-4 sm:p-6 bg-white dark:bg-gray-800 rounded-lg shadow-lg">
      <h2 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">Agent Credentials</h2>
      <p className="text-red-600 dark:text-red-300">
      WIP
      </p>
      {agentId && (
        <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
          Agent ID: {agentId}
        </p>
      )}
    </div>
  );
}

export default AgentCredentialsTab;