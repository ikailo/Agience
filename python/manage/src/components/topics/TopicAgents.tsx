interface Agent {
  id: string;
  name: string;
  description: string;
}

export const TopicAgents = () => {
  // Placeholder data - will be replaced with API call later
  const agents = [
    {
      id: '1',
      name: 'Astra',
      description: 'General purpose AI assistant for daily tasks',
    },
    {
      id: '2',
      name: 'Nova',
      description: 'Specialized AI for data analysis and visualization',
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Connected Agents
        </h2>
      </div>

      {/* Desktop view */}
      <div className="hidden md:block overflow-x-auto">
        <table className="min-w-full">
          <thead className="bg-gray-900 dark:bg-gray-800">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                Agent Name
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                Description
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-gray-800 dark:bg-gray-900 divide-y divide-gray-700">
            {agents.map(agent => (
              <tr key={agent.id} className="hover:bg-gray-700 transition-colors">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                  {agent.name}
                </td>
                <td className="px-6 py-4 text-sm text-gray-300">
                  {agent.description}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                  <button
                    onClick={() => {/* TODO: Implement disconnect logic */}}
                    className="text-red-400 hover:text-red-300"
                  >
                    Disconnect
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile view */}
      <div className="md:hidden space-y-4">
        {agents.map(agent => (
          <div 
            key={agent.id} 
            className="bg-gray-800 rounded-lg p-4 space-y-3"
          >
            <div>
              <h3 className="text-white font-medium">{agent.name}</h3>
              <p className="text-gray-400 text-sm">{agent.description}</p>
            </div>
            <div className="flex justify-end pt-2">
              <button
                onClick={() => {/* TODO: Implement disconnect logic */}}
                className="text-red-400 hover:text-red-300"
              >
                Disconnect
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}; 