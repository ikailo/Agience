interface Host {
  id: string;
  name: string;
  description: string;
  redirectUris: string;
  postLogoutUris: string;
  scopes: string[];
}

interface HostTableProps {
  hosts: Host[];
  onEdit: (host: Host) => void;
  onDelete: (id: string) => void;
}

export const HostTable: React.FC<HostTableProps> = ({
  hosts,
  onEdit,
  onDelete,
}) => {
  return (
    <>
      {/* Desktop view */}
      <div className="hidden md:block overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Name
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Description
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Redirect URIs
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Post Logout URIs
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Scopes
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200 dark:bg-gray-900 dark:divide-gray-700">
            {hosts.map(host => (
              <tr key={host.id} className="hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                  {host.name}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-300">
                  {host.description}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-300">
                  {host.redirectUris}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-300">
                  {host.postLogoutUris}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-300">
                  {host.scopes.join(', ')}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                  <button
                    onClick={() => onEdit(host)}
                    className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 mr-4"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => onDelete(host.id)}
                    className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile view */}
      <div className="md:hidden space-y-4">
        {hosts.map(host => (
          <div 
            key={host.id} 
            className="bg-white dark:bg-gray-800 rounded-lg p-4 space-y-3 shadow-sm border border-gray-200 dark:border-gray-700"
          >
            <div>
              <h3 className="text-gray-900 dark:text-white font-medium">{host.name}</h3>
              <p className="text-gray-500 dark:text-gray-400 text-sm mt-1">{host.description}</p>
            </div>
            
            <div className="space-y-2">
              <div>
                <label className="text-xs font-medium text-gray-500 dark:text-gray-400">
                  Redirect URIs
                </label>
                <p className="text-gray-900 dark:text-gray-300 text-sm mt-1">
                  {host.redirectUris}
                </p>
              </div>
              
              <div>
                <label className="text-xs font-medium text-gray-500 dark:text-gray-400">
                  Post Logout URIs
                </label>
                <p className="text-gray-900 dark:text-gray-300 text-sm mt-1">
                  {host.postLogoutUris}
                </p>
              </div>
              
              <div>
                <label className="text-xs font-medium text-gray-500 dark:text-gray-400">
                  Scopes
                </label>
                <p className="text-gray-900 dark:text-gray-300 text-sm mt-1">
                  {host.scopes.join(', ')}
                </p>
              </div>
            </div>

            <div className="flex justify-end pt-2 space-x-4">
              <button
                onClick={() => onEdit(host)}
                className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
              >
                Edit
              </button>
              <button
                onClick={() => onDelete(host.id)}
                className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>
    </>
  );
}; 