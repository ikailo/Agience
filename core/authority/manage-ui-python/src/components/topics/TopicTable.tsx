import { useState } from 'react';

interface Topic {
  id: string;
  name: string;
  description: string;
  address: string;
}

interface TopicTableProps {
  topics: Topic[];
  onEdit: (id: string, topic: Topic) => void;
  onDelete: (id: string) => void;
}

export const TopicTable: React.FC<TopicTableProps> = ({
  topics,
  onEdit,
  onDelete,
}) => {
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState<Topic | null>(null);

  const handleEditClick = (topic: Topic) => {
    setEditingId(topic.id);
    setEditForm(topic);
  };

  const handleSave = () => {
    if (editForm && editingId) {
      onEdit(editingId, editForm);
      setEditingId(null);
      setEditForm(null);
    }
  };

  const handleCancel = () => {
    setEditingId(null);
    setEditForm(null);
  };

  return (
    <>
      {/* Desktop view */}
      <div className="hidden md:block overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">Topic</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">Description</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">Address</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200 dark:bg-gray-900 dark:divide-gray-700">
            {topics.map(topic => (
              <tr key={topic.id} className="hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                  {editingId === topic.id ? (
                    <input
                      type="text"
                      value={editForm?.name}
                      onChange={e => setEditForm(prev => ({ ...prev!, name: e.target.value }))}
                      className="w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-white rounded border-gray-300 dark:border-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                    />
                  ) : topic.name}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-300">
                  {editingId === topic.id ? (
                    <input
                      type="text"
                      value={editForm?.description}
                      onChange={e => setEditForm(prev => ({ ...prev!, description: e.target.value }))}
                      className="w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-white rounded border-gray-300 dark:border-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                    />
                  ) : topic.description}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-300">
                  {editingId === topic.id ? (
                    <input
                      type="text"
                      value={editForm?.address}
                      onChange={e => setEditForm(prev => ({ ...prev!, address: e.target.value }))}
                      className="w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-white rounded border-gray-300 dark:border-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                    />
                  ) : topic.address}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                  {editingId === topic.id ? (
                    <div className="flex justify-end space-x-2">
                      <button
                        onClick={handleSave}
                        className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 font-medium"
                      >
                        Save
                      </button>
                      <button
                        onClick={handleCancel}
                        className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 font-medium"
                      >
                        Cancel
                      </button>
                    </div>
                  ) : (
                    <div className="flex justify-end space-x-4">
                      <button
                        onClick={() => handleEditClick(topic)}
                        className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => onDelete(topic.id)}
                        className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                      >
                        Delete
                      </button>
                    </div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile view */}
      <div className="md:hidden space-y-4">
        {topics.map(topic => (
          <div 
            key={topic.id} 
            className="bg-white dark:bg-gray-800 rounded-lg p-4 space-y-3 shadow-sm border border-gray-200 dark:border-gray-700"
          >
            {editingId === topic.id ? (
              <>
                <input
                  type="text"
                  value={editForm?.name}
                  onChange={e => setEditForm(prev => ({ ...prev!, name: e.target.value }))}
                  className="w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-white rounded border-gray-300 dark:border-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 mb-2"
                  placeholder="Name"
                />
                <input
                  type="text"
                  value={editForm?.description}
                  onChange={e => setEditForm(prev => ({ ...prev!, description: e.target.value }))}
                  className="w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-white rounded border-gray-300 dark:border-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 mb-2"
                  placeholder="Description"
                />
                <input
                  type="text"
                  value={editForm?.address}
                  onChange={e => setEditForm(prev => ({ ...prev!, address: e.target.value }))}
                  className="w-full bg-white dark:bg-gray-700 text-gray-900 dark:text-white rounded border-gray-300 dark:border-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                  placeholder="Address"
                />
              </>
            ) : (
              <>
                <h3 className="text-gray-900 dark:text-white font-medium">{topic.name}</h3>
                <p className="text-gray-500 dark:text-gray-400 text-sm">{topic.description}</p>
                <p className="text-gray-500 dark:text-gray-400 text-sm font-mono">{topic.address}</p>
              </>
            )}
            
            <div className="flex justify-end pt-2 space-x-4">
              {editingId === topic.id ? (
                <>
                  <button
                    onClick={handleSave}
                    className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                  >
                    Save
                  </button>
                  <button
                    onClick={handleCancel}
                    className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
                  >
                    Cancel
                  </button>
                </>
              ) : (
                <>
                  <button
                    onClick={() => handleEditClick(topic)}
                    className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => onDelete(topic.id)}
                    className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                  >
                    Delete
                  </button>
                </>
              )}
            </div>
          </div>
        ))}
      </div>
    </>
  );
}; 