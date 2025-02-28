import { AgentDetailsForm, AgentFormData } from './AgentDetailsForm';
import { AgentList } from './AgentList';

export const AgentDetailsTab: React.FC = () => {
  const handleSave = (data: AgentFormData) => {
    console.log('Saving:', data);
    // Implement save logic
  };

  const handleDelete = () => {
    console.log('Deleting agent');
    // Implement delete logic
  };

  const handleSelectAgent = (id: string) => {
    console.log('Selected agent:', id);
    // Implement selection logic
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <div>
        <AgentDetailsForm
          onSave={handleSave}
          onDelete={handleDelete}
        />
      </div>
      <div className="mt-8 lg:mt-0">
        <AgentList onSelectAgent={handleSelectAgent} />
      </div>
    </div>
  );
}; 