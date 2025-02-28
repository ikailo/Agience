import { AgentTopicsTable } from './AgentTopicsTable';

// Dummy data
const dummyTopics = [
  {
    id: '1',
    name: 'System Events',
    description: 'Handles system-wide event notifications',
    address: '/topics/system-events',
  },
  {
    id: '2',
    name: 'User Interactions',
    description: 'Manages user interaction events and responses',
    address: '/topics/user-interactions',
  },
];

export const AgentTopicsTab: React.FC = () => {
  const handleEdit = (id: string) => {
    console.log('Edit topic:', id);
    // Implement edit logic
  };

  const handleDelete = (id: string) => {
    console.log('Delete topic:', id);
    // Implement delete logic
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-medium text-gray-900 dark:text-white">
          Connected Topics
        </h2>
        <button className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700">
          Add Topic
        </button>
      </div>
      
      {/* Mobile view */}
      <div className="sm:hidden">
        {dummyTopics.map((topic) => (
          <div key={topic.id} className="bg-white dark:bg-gray-800 shadow rounded-lg p-4 mb-4">
            <h3 className="font-medium text-gray-900 dark:text-white">{topic.name}</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{topic.description}</p>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{topic.address}</p>
            <div className="mt-4 flex justify-end space-x-4">
              <button
                onClick={() => handleEdit(topic.id)}
                className="text-blue-600 dark:text-blue-400"
              >
                Edit
              </button>
              <button
                onClick={() => handleDelete(topic.id)}
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
        <AgentTopicsTable
          topics={dummyTopics}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />
      </div>
    </div>
  );
}; 