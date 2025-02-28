interface Credential {
  id: string;
  connection: string;
  authorizer: string;
  authorize: boolean;
  status: 'active' | 'inactive' | 'pending';
}

interface AgentCredentialsTableProps {
  credentials: Credential[];
  onToggleAuthorize: (id: string) => void;
}

export const AgentCredentialsTable: React.FC<AgentCredentialsTableProps> = ({
  credentials,
  onToggleAuthorize,
}) => {
  const getStatusColor = (status: Credential['status']) => {
    switch (status) {
      case 'active':
        return 'text-green-600 dark:text-green-400';
      case 'inactive':
        return 'text-red-600 dark:text-red-400';
      case 'pending':
        return 'text-yellow-600 dark:text-yellow-400';
      default:
        return 'text-gray-600 dark:text-gray-400';
    }
  };

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
        <thead className="bg-gray-50 dark:bg-gray-800">
          <tr>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Connection
            </th>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Authorizer
            </th>
            <th scope="col" className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Authorize
            </th>
            <th scope="col" className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Status
            </th>
          </tr>
        </thead>
        <tbody className="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
          {credentials.map((credential) => (
            <tr key={credential.id} className="hover:bg-gray-50 dark:hover:bg-gray-800">
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                {credential.connection}
              </td>
              <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                {credential.authorizer}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-center">
                <button
                  onClick={() => onToggleAuthorize(credential.id)}
                  className={`px-3 py-1 rounded-full text-sm font-medium
                    ${credential.authorize 
                      ? 'bg-blue-100 text-blue-600 dark:bg-blue-900 dark:text-blue-400'
                      : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400'
                    }`}
                >
                  {credential.authorize ? 'Authorized' : 'Unauthorized'}
                </button>
              </td>
              <td className={`px-6 py-4 whitespace-nowrap text-sm text-center ${getStatusColor(credential.status)}`}>
                {credential.status.charAt(0).toUpperCase() + credential.status.slice(1)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}; 