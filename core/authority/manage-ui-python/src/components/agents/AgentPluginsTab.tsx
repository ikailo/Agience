import { AgentPluginsTable } from './AgentPluginsTable';

// Add this interface at the top of the file
interface AgentPluginsTabProps {
  agentId: string | null;
}

// Dummy data
const dummyPlugins = [
  {
    id: '1',
    name: 'OpenAI Plugin',
    description: 'Integrates with OpenAI API for enhanced language processing capabilities',
  },
  {
    id: '2',
    name: 'Database Connector',
    description: 'Enables connection to various database systems for data storage and retrieval',
  },
];

export const AgentPluginsTab: React.FC<AgentPluginsTabProps> = ({ agentId }) => {
  const handleEdit = (id: string) => {
    console.log('Edit plugin:', id);
    // Implement edit logic
  };

  const handleDelete = (id: string) => {
    console.log('Delete plugin:', id);
    // Implement delete logic
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-medium text-gray-900 dark:text-white">
          Installed Plugins
        </h2>
        <button className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700">
          Add Plugin
        </button>
      </div>
      
      {/* Mobile view */}
      <div className="sm:hidden">
        {dummyPlugins.map((plugin) => (
          <div key={plugin.id} className="bg-white dark:bg-gray-800 shadow rounded-lg p-4 mb-4">
            <h3 className="font-medium text-gray-900 dark:text-white">{plugin.name}</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{plugin.description}</p>
            <div className="mt-4 flex justify-end space-x-4">
              <button
                onClick={() => handleEdit(plugin.id)}
                className="text-blue-600 dark:text-blue-400"
              >
                Edit
              </button>
              <button
                onClick={() => handleDelete(plugin.id)}
                className="text-red-600 dark:text-red-400"
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Desktop view */}
      <div className="hidden sm:block">
        <AgentPluginsTable
          plugins={dummyPlugins}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />
      </div>
    </div>
  );
}; 