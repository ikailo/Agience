interface Plugin {
  id: string;
  name: string;
  description: string;
  status: 'active' | 'inactive';
  type: 'communication' | 'utility' | 'ai';
}

export const HostPlugins = () => {
  const samplePlugins: Plugin[] = [
    {
      id: '1',
      name: 'Gmail Integration',
      description: 'Enables email communication and inbox management through Gmail',
      status: 'active',
      type: 'communication',
    },
    {
      id: '2',
      name: 'Speech-to-Text',
      description: 'Converts spoken language to written text using advanced AI',
      status: 'active',
      type: 'ai',
    },
    {
      id: '3',
      name: 'Calendar Sync',
      description: 'Synchronizes events and schedules across platforms',
      status: 'inactive',
      type: 'utility',
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Installed Plugins
        </h2>
        <button className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">
          Add Plugin
        </button>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {samplePlugins.map(plugin => (
          <div 
            key={plugin.id}
            className="bg-gray-800 rounded-lg p-4 space-y-3"
          >
            <div className="flex justify-between items-start">
              <div>
                <h3 className="text-white font-medium">{plugin.name}</h3>
                <span className={`inline-block px-2 py-1 text-xs rounded-full mt-2 ${
                  plugin.type === 'communication' ? 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300' :
                  plugin.type === 'ai' ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300' :
                  'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                }`}>
                  {plugin.type}
                </span>
              </div>
              <span className={`px-2 py-1 rounded-full text-xs ${
                plugin.status === 'active' 
                  ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
                  : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'
              }`}>
                {plugin.status}
              </span>
            </div>
            
            <p className="text-gray-400 text-sm">
              {plugin.description}
            </p>

            <div className="flex justify-end pt-2 space-x-3">
              <button className="text-blue-400 hover:text-blue-300">
                Configure
              </button>
              <button className="text-red-400 hover:text-red-300">
                Remove
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}; 