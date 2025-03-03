import { useState } from 'react';
import { Button } from '../common/Button';
import { CredentialModal } from './CredentialModal';
import { Table } from '../common/Table';

interface Credential {
  id: string;
  name: string;
  description: string;
  authorizerId: string;
  authorizerName: string;
  authorizerType: string;
  createdAt: string;
}

interface Authorizer {
  id: string;
  name: string;
  description: string;
  type: 'public' | 'API Key' | 'OAuth';
}

interface AgentCredentialsProps {
  isModalOpen?: boolean;
  setIsModalOpen?: (isOpen: boolean) => void;
  onSaveCredential?: (credential: any) => void;
}

export const AgentCredentials: React.FC<AgentCredentialsProps> = ({
  isModalOpen: externalIsModalOpen,
  setIsModalOpen: externalSetIsModalOpen,
  onSaveCredential
}) => {
  // Use local state if no external state is provided
  const [localIsModalOpen, setLocalIsModalOpen] = useState(false);
  
  // Use either the external or local state
  const isModalOpen = externalIsModalOpen !== undefined ? externalIsModalOpen : localIsModalOpen;
  const setIsModalOpen = externalSetIsModalOpen || setLocalIsModalOpen;
  
  const [credentials, setCredentials] = useState<Credential[]>([
    {
      id: '1',
      name: 'Google API Key',
      description: 'Used for Google Maps integration',
      authorizerId: 'auth_1',
      authorizerName: 'Google Cloud',
      authorizerType: 'API Key',
      createdAt: '2023-10-15'
    },
    {
      id: '2',
      name: 'OpenAI API Key',
      description: 'For AI model access',
      authorizerId: 'auth_2',
      authorizerName: 'OpenAI',
      authorizerType: 'API Key',
      createdAt: '2023-11-20'
    }
  ]);
  
  const [authorizers] = useState<Authorizer[]>([
    {
      id: 'auth_1',
      name: 'Google Cloud',
      description: 'Google Cloud Platform API',
      type: 'API Key'
    },
    {
      id: 'auth_2',
      name: 'OpenAI',
      description: 'OpenAI Platform',
      type: 'API Key'
    },
    {
      id: 'auth_3',
      name: 'GitHub',
      description: 'GitHub API',
      type: 'OAuth'
    }
  ]);
  
  const handleSaveCredential = (credentialData: any) => {
    if (onSaveCredential) {
      onSaveCredential(credentialData);
    } else {
      // Find the authorizer to get its name and type
      const authorizer = authorizers.find(a => a.id === credentialData.authorizerId);
      
      // Create a new credential with the data
      const newCredential: Credential = {
        id: `cred_${Date.now()}`,
        name: credentialData.name,
        description: credentialData.description,
        authorizerId: credentialData.authorizerId,
        authorizerName: authorizer?.name || 'Unknown',
        authorizerType: authorizer?.type || 'Unknown',
        createdAt: new Date().toISOString().split('T')[0]
      };
      
      // Add to the list
      setCredentials([...credentials, newCredential]);
    }
  };
  
  const handleDeleteCredential = (id: string) => {
    setCredentials(credentials.filter(cred => cred.id !== id));
  };
  
  const handleOpenModal = () => {
    console.log('Opening modal, current state:', isModalOpen);
    setIsModalOpen(true);
    console.log('New state should be:', true);
  };
  
  const columns = [
    { header: 'Name', accessor: 'name' },
    { header: 'Description', accessor: 'description' },
    { header: 'Authorizer', accessor: 'authorizerName' },
    { header: 'Type', accessor: 'authorizerType' },
    { header: 'Created', accessor: 'createdAt' }
  ];
  
  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Agent Credentials
        </h2>
        <Button
          variant="primary"
          onClick={handleOpenModal}
        >
          Add Credential
        </Button>
      </div>
      
      <Table
        columns={columns}
        data={credentials}
        actions={(credential) => (
          <button
            onClick={() => handleDeleteCredential(credential.id)}
            className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
          >
            Delete
          </button>
        )}
      />
      
      <CredentialModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSaveCredential}
        existingAuthorizers={authorizers}
      />
    </div>
  );
}; 