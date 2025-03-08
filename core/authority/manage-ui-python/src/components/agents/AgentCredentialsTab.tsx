import React, { useState, useEffect } from 'react';
import { Agent } from '../../types/Agent';
import { agentService } from '../../services/api/agentService';

interface AgentCredentialsTabProps {
  agentId?: string;
}

/**
 * Component for managing agent credentials
 */
function AgentCredentialsTab({ agentId }: AgentCredentialsTabProps) {
  const [selectedAgent, setSelectedAgent] = useState<Agent | null>(null);

  useEffect(() => {
    let isMounted = true; // Track if the component is mounted

    const fetchAgentDetails = async () => {
      if (agentId) {
        try {
          const agent = await agentService.getAgentById(agentId);
          if (isMounted) {
            // Only update state if the component is still mounted
            setSelectedAgent(agent);
          }
        } catch (error) {
          if (isMounted) {
            // Handle error by clearing the selected agent
            setSelectedAgent(null);
            // console.error('Failed to fetch agent details:', error);
          }
        }
      }
    };

    fetchAgentDetails();
    // Cleanup function to set isMounted to false when the component unmounts
    return () => {
      isMounted = false;
    };
  }, [agentId]);

  return (
    <div className="p-4 sm:p-6 bg-white dark:bg-gray-800 rounded-lg shadow-lg">
      <h2 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">Agent Credentials</h2>
      {selectedAgent ? (
        <p className="text-gray-600 dark:text-gray-300">
          Credential management for <span className="font-medium">{selectedAgent.name}</span> will be implemented soon.
        </p>
      ) : (
        <p className="text-gray-600 dark:text-gray-300">
          Credential management functionality will be implemented soon.
        </p>
      )}
      {agentId && (
        <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
          Agent ID: {agentId}
        </p>
      )}
    </div>
  );
}

export default AgentCredentialsTab;