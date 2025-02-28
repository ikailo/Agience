import { useState } from 'react';
import { Button } from '../common/Button';
import { HostTable } from './HostTable';
import { HostForm } from './HostForm';

interface Host {
  id: string;
  name: string;
  description: string;
  redirectUris: string;
  postLogoutUris: string;
  scopes: string[];
}

export const HostDetails = () => {
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingHost, setEditingHost] = useState<Host | null>(null);
  // These are sample hosts for now
  const [hosts, setHosts] = useState<Host[]>([
    {
      id: '1',
      name: 'Development Server',
      description: 'Local development environment',
      redirectUris: 'http://localhost:3000/callback',
      postLogoutUris: 'http://localhost:3000',
      scopes: ['openid', 'profile', 'email'],
    },
    //
  ]);

  const handleEdit = (host: Host) => {
    setEditingHost(host);
    setIsFormOpen(true);
  };

  const handleDelete = (id: string) => {
    setHosts(hosts.filter(host => host.id !== id));
  };

  const handleSave = (host: Host) => {
    if (editingHost) {
      setHosts(hosts.map(h => h.id === host.id ? host : h));
    } else {
      setHosts([...hosts, { ...host, id: Date.now().toString() }]);
    }
    setIsFormOpen(false);
    setEditingHost(null);
  };

  const handleAdd = () => {
    setEditingHost(null);
    setIsFormOpen(true);
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Hosts
        </h2>
        <Button
          variant="primary"
          onClick={handleAdd}
        >
          Add Host
        </Button>
      </div>

      <HostTable
        hosts={hosts}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      {isFormOpen && (
        <HostForm
          host={editingHost}
          onSave={handleSave}
          onCancel={() => {
            setIsFormOpen(false);
            setEditingHost(null);
          }}
        />
      )}
    </div>
  );
}; 