import { AgentCredentialsTable } from './AgentCredentialsTable';

// Dummy data
const dummyCredentials = [
  {
    id: '1',
    connection: 'Database',
    authorizer: 'System Admin',
    authorize: true,
    status: 'active' as const,
  },
  {
    id: '2',
    connection: 'API Gateway',
    authorizer: 'Service Account',
    authorize: false,
    status: 'pending' as const,
  },
];

export const AgentCredentialsTab: React.FC = () => {
  const handleToggleAuthorize = (id: string) => {
    console.log('Toggle authorize for:', id);
    // Implement toggle logic
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-medium text-gray-900 dark:text-white">
          Credentials Management
        </h2>
        <button className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700">
          Add Credential
        </button>
      </div>
      
      {/* Mobile view */}
      <div className="sm:hidden">
        {dummyCredentials.map((credential) => (
          <div key={credential.id} className="bg-white dark:bg-gray-800 shadow rounded-lg p-4 mb-4">
            <div className="flex justify-between items-start">
              <div>
                <h3 className="font-medium text-gray-900 dark:text-white">{credential.connection}</h3>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{credential.authorizer}</p>
              </div>
              <span className={`px-2 py-1 text-xs rounded-full ${
                credential.status === 'active' ? 'bg-green-100 text-green-600 dark:bg-green-900 dark:text-green-400' :
                credential.status === 'pending' ? 'bg-yellow-100 text-yellow-600 dark:bg-yellow-900 dark:text-yellow-400' :
                'bg-red-100 text-red-600 dark:bg-red-900 dark:text-red-400'
              }`}>
                {credential.status.charAt(0).toUpperCase() + credential.status.slice(1)}
              </span>
            </div>
            <div className="mt-4">
              <button
                onClick={() => handleToggleAuthorize(credential.id)}
                className={`w-full py-2 rounded text-sm font-medium
                  ${credential.authorize 
                    ? 'bg-blue-100 text-blue-600 dark:bg-blue-900 dark:text-blue-400'
                    : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400'
                  }`}
              >
                {credential.authorize ? 'Authorized' : 'Unauthorized'}
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Desktop view */}
      <div className="hidden sm:block">
        <AgentCredentialsTable
          credentials={dummyCredentials}
          onToggleAuthorize={handleToggleAuthorize}
        />
      </div>
    </div>
  );
}; 