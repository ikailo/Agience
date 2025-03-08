import { useState } from 'react';
import Button  from '../common/Button';

interface Host {
  id: string;
  name: string;
  description: string;
  redirectUris: string;
  postLogoutUris: string;
  scopes: string[];
}

interface HostFormProps {
  host: Host | null;
  onSave: (host: Host) => void;
  onCancel: () => void;
}

const AVAILABLE_SCOPES = ['openid', 'manage', 'profile', 'email', 'connect'];

export const HostForm = ({ host, onSave, onCancel }: HostFormProps) => {
  const [formData, setFormData] = useState<Omit<Host, 'id'>>({
    name: host?.name || '',
    description: host?.description || '',
    redirectUris: host?.redirectUris || '',
    postLogoutUris: host?.postLogoutUris || '',
    scopes: host?.scopes || [],
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave({
      id: host?.id || '',
      ...formData,
    });
  };

  const handleScopeChange = (scope: string) => {
    setFormData(prev => ({
      ...prev,
      scopes: prev.scopes.includes(scope)
        ? prev.scopes.filter(s => s !== scope)
        : [...prev.scopes, scope],
    }));
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center">
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-2xl">
        <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
          {host ? 'Edit Host' : 'Add Host'}
        </h3>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Name
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={e => setFormData(prev => ({ ...prev, name: e.target.value }))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Description
            </label>
            <input
              type="text"
              value={formData.description}
              onChange={e => setFormData(prev => ({ ...prev, description: e.target.value }))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Redirect URIs
            </label>
            <input
              type="text"
              value={formData.redirectUris}
              onChange={e => setFormData(prev => ({ ...prev, redirectUris: e.target.value }))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Post Logout URIs
            </label>
            <input
              type="text"
              value={formData.postLogoutUris}
              onChange={e => setFormData(prev => ({ ...prev, postLogoutUris: e.target.value }))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Scopes
            </label>
            <div className="space-y-2">
              {AVAILABLE_SCOPES.map(scope => (
                <label key={scope} className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.scopes.includes(scope)}
                    onChange={() => handleScopeChange(scope)}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500 dark:border-gray-600"
                  />
                  <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    {scope}
                  </span>
                </label>
              ))}
            </div>
          </div>

          <div className="flex justify-end space-x-3 mt-6">
            <Button variant="secondary" onClick={onCancel}>
              Cancel
            </Button>
            <Button variant="primary" type="submit">
              {host ? 'Save Changes' : 'Add Host'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}; 