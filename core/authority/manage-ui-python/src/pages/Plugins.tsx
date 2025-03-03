import { useState } from 'react';
import { PluginDetails } from '../components/plugins/PluginDetails';

// import { PluginFunctions } from '../components/plugins/PluginFunctions';
// import { PluginConnections } from '../components/plugins/PluginConnections';

const Plugins = () => {
  const [activeTab, setActiveTab] = useState('details');

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Plugin Configuration</h1>

      {/* Tab Navigation */}
      <div className="mb-4 border-b border-gray-200 dark:border-gray-700">
        <ul className="flex flex-wrap -mb-px">
          <li className="mr-2">
            <button
              onClick={() => setActiveTab('details')}
              className={`inline-block p-4 border-b-2 rounded-t-lg ${
                activeTab === 'details'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
              }`}
            >
              Details
            </button>
          </li>
          <li className="mr-2">
            <button
              onClick={() => setActiveTab('functions')}
              className={`inline-block p-4 border-b-2 rounded-t-lg ${
                activeTab === 'functions'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
              }`}
            >
              Functions
            </button>
          </li>
          <li className="mr-2">
            <button
              onClick={() => setActiveTab('connections')}
              className={`inline-block p-4 border-b-2 rounded-t-lg ${
                activeTab === 'connections'
                  ? 'text-blue-600 border-blue-600 dark:text-blue-500 dark:border-blue-500'
                  : 'border-transparent hover:text-gray-600 hover:border-gray-300 dark:hover:text-gray-300'
              }`}
            >
              Connections
            </button>
          </li>
        </ul>
      </div>

      {/* Tab Content */}
      <div className="mt-4">
        {activeTab === 'details' && <PluginDetails />}
        {/* {activeTab === 'functions' && <PluginFunctions />} */}
        {/* {activeTab === 'connections' && <PluginConnections />} */}
      </div>
    </div>
  );
};

export default Plugins;
