interface HostKey {
  id: string;
  keyId: string;
  secret: string;
  created: string;
  lastUsed: string;
  status: 'active' | 'expired';
}

export const HostKeys = () => {
  const sampleKeys: HostKey[] = [
    {
      id: '1',
      keyId: 'key_h7x9m2p4v5',
      secret: '••••••••••••3kj9',
      created: '2024-02-15',
      lastUsed: '2024-03-20',
      status: 'active',
    },
    {
      id: '2',
      keyId: 'key_n3k7r9t2w4',
      secret: '••••••••••••8qp5',
      created: '2024-01-10',
      lastUsed: '2024-03-18',
      status: 'active',
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Host Keys
        </h2>
        <button className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">
          Generate New Key
        </button>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full">
          <thead className="bg-gray-900 dark:bg-gray-800">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Key ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Secret</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Created</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Last Used</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-gray-800 dark:bg-gray-900 divide-y divide-gray-700">
            {sampleKeys.map(key => (
              <tr key={key.id} className="hover:bg-gray-700 transition-colors">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-white font-mono">
                  {key.keyId}
                </td>
                <td className="px-6 py-4 text-sm text-gray-300 font-mono">
                  {key.secret}
                </td>
                <td className="px-6 py-4 text-sm text-gray-300">
                  {key.created}
                </td>
                <td className="px-6 py-4 text-sm text-gray-300">
                  {key.lastUsed}
                </td>
                <td className="px-6 py-4 text-sm">
                  <span className={`px-2 py-1 rounded-full text-xs ${
                    key.status === 'active' 
                      ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
                      : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'
                  }`}>
                    {key.status}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                  <button className="text-red-400 hover:text-red-300">
                    Revoke
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}; 