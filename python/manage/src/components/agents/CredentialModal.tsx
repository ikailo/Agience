import { useState } from 'react';
import { Button } from '../common/Button';

interface Authorizer {
  id: string;
  name: string;
  description: string;
  type: 'public' | 'API Key' | 'OAuth';
}

interface Credential {
  name: string;
  description: string;
  authorizerId: string;
  value: string;
}

interface CredentialModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (credential: Credential) => void;
  existingAuthorizers?: Authorizer[];
}

export const CredentialModal: React.FC<CredentialModalProps> = ({
  isOpen,
  onClose,
  onSave,
  existingAuthorizers = []
}) => {
  console.log('Modal props:', { isOpen, onClose, existingAuthorizers });
  
  const [credentialData, setCredentialData] = useState<Credential>({
    name: '',
    description: '',
    authorizerId: '',
    value: ''
  });
  
  const [showAuthorizerForm, setShowAuthorizerForm] = useState(false);
  const [authorizerData, setAuthorizerData] = useState<Omit<Authorizer, 'id'>>({
    name: '',
    description: '',
    type: 'public'
  });
  
  const [authorizers, setAuthorizers] = useState<Authorizer[]>(existingAuthorizers);
  
  const handleCredentialChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setCredentialData({
      ...credentialData,
      [e.target.name]: e.target.value
    });
  };
  
  const handleAuthorizerChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setAuthorizerData({
      ...authorizerData,
      [e.target.name]: e.target.value as any
    });
  };
  
  const handleCreateAuthorizer = () => {
    // Create a new authorizer with a unique ID
    const newAuthorizer: Authorizer = {
      id: `auth_${Date.now()}`,
      ...authorizerData
    };
    
    // Add to list and select it
    const updatedAuthorizers = [...authorizers, newAuthorizer];
    setAuthorizers(updatedAuthorizers);
    setCredentialData({
      ...credentialData,
      authorizerId: newAuthorizer.id
    });
    
    // Hide the form
    setShowAuthorizerForm(false);
  };
  
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(credentialData);
    
    // Reset form
    setCredentialData({
      name: '',
      description: '',
      authorizerId: '',
      value: ''
    });
    
    onClose();
  };
  
  console.log('Should render modal?', isOpen);
  
  if (!isOpen) return null;
  
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-card-light dark:bg-card-dark rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
              Add New Credential
            </h2>
            <button 
              onClick={onClose}
              className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
              aria-label="Close"
            >
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          
          <form onSubmit={handleSubmit}>
            {/* Credential Form */}
            <div className="space-y-4 mb-6">
              <div>
                <label htmlFor="credential-name" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Credential Name
                </label>
                <input
                  id="credential-name"
                  type="text"
                  name="name"
                  value={credentialData.name}
                  onChange={handleCredentialChange}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                  required
                />
              </div>
              
              <div>
                <label htmlFor="credential-description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Description
                </label>
                <textarea
                  id="credential-description"
                  name="description"
                  value={credentialData.description}
                  onChange={handleCredentialChange}
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                />
              </div>
              
              <div>
                <label htmlFor="credential-value" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Credential Value
                </label>
                <input
                  id="credential-value"
                  type="password"
                  name="value"
                  value={credentialData.value}
                  onChange={handleCredentialChange}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                  required
                />
              </div>
              
              {/* Authorizer Selection */}
              <div>
                <div className="flex justify-between items-center mb-1">
                  <label htmlFor="authorizer-select" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                    Authorizer
                  </label>
                  <button
                    type="button"
                    onClick={() => setShowAuthorizerForm(true)}
                    className="text-sm text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                  >
                    + Create New
                  </button>
                </div>
                
                <select
                  id="authorizer-select"
                  name="authorizerId"
                  value={credentialData.authorizerId}
                  onChange={handleCredentialChange}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                  required
                >
                  <option value="">Select an authorizer</option>
                  {authorizers.map(auth => (
                    <option key={auth.id} value={auth.id}>
                      {auth.name} ({auth.type})
                    </option>
                  ))}
                </select>
              </div>
            </div>
            
            {/* Create Authorizer Form */}
            {showAuthorizerForm && (
              <div className="border-t border-gray-200 dark:border-gray-700 pt-6 mb-6">
                <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
                  Create New Authorizer
                </h3>
                
                <div className="space-y-4">
                  <div>
                    <label htmlFor="authorizer-name" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Authorizer Name
                    </label>
                    <input
                      id="authorizer-name"
                      type="text"
                      name="name"
                      value={authorizerData.name}
                      onChange={handleAuthorizerChange}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                      required
                    />
                  </div>
                  
                  <div>
                    <label htmlFor="authorizer-description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Description
                    </label>
                    <textarea
                      id="authorizer-description"
                      name="description"
                      value={authorizerData.description}
                      onChange={handleAuthorizerChange}
                      rows={2}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                    />
                  </div>
                  
                  <div>
                    <label htmlFor="authorizer-type" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Authorizer Type
                    </label>
                    <select
                      id="authorizer-type"
                      name="type"
                      value={authorizerData.type}
                      onChange={handleAuthorizerChange}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                    >
                      <option value="public">Public</option>
                      <option value="API Key">API Key</option>
                      <option value="OAuth">OAuth</option>
                    </select>
                  </div>
                  
                  <div className="flex justify-end space-x-3 pt-2">
                    <Button
                      variant="secondary"
                      onClick={() => setShowAuthorizerForm(false)}
                    >
                      Cancel
                    </Button>
                    <Button
                      variant="primary"
                      onClick={handleCreateAuthorizer}
                      disabled={!authorizerData.name}
                    >
                      Create Authorizer
                    </Button>
                  </div>
                </div>
              </div>
            )}
            
            {/* Modal Actions */}
            <div className="flex justify-end space-x-3 pt-4 border-t border-gray-200 dark:border-gray-700">
              <Button
                variant="secondary"
                onClick={onClose}
                type="button"
              >
                Cancel
              </Button>
              <Button
                variant="primary"
                type="submit"
                disabled={!credentialData.name || !credentialData.value || !credentialData.authorizerId}
              >
                Save Credential
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}; 