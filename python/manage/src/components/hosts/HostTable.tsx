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
    <div className="overflow-x-auto">
      <table className="min-w-full">
        <thead className="bg-gray-900 dark:bg-gray-800">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Name</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Description</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Redirect URIs</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Post Logout URIs</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Scopes</th>
            <th className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">Actions</th>
          </tr>
        </thead>
        <tbody className="bg-gray-800 dark:bg-gray-900 divide-y divide-gray-700">
          {hosts.map(host => (
            <tr key={host.id} className="hover:bg-gray-700 transition-colors">
              <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                {host.name}
              </td>
              <td className="px-6 py-4 text-sm text-gray-300">
                {host.description}
              </td>
              <td className="px-6 py-4 text-sm text-gray-300">
                {host.redirectUris}
              </td>
              <td className="px-6 py-4 text-sm text-gray-300">
                {host.postLogoutUris}
              </td>
              <td className="px-6 py-4 text-sm text-gray-300">
                {host.scopes.join(', ')}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                <button
                  onClick={() => onEdit(host)}
                  className="text-blue-400 hover:text-blue-300 mr-4"
                >
                  Edit
                </button>
                <button
                  onClick={() => onDelete(host.id)}
                  className="text-red-400 hover:text-red-300"
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}; 