import { AgentCard } from './AgentCard';

// Dummy data
const dummyAgents = [
  {
    id: '1',
    name: 'Astra',
    description: 'AI assistant for Google Calendar scheduling and email management',
    imageUrl: '/astra-avatar.png', // Make sure to add a default image in public folder
  },
  {
    id: '2',
    name: 'Nova',
    description: 'Document analysis and data extraction specialist',
    imageUrl: '/nova-avatar.webp', // Make sure to add a default image in public folder
  },
];

interface AgentListProps {
  onSelectAgent: (id: string) => void;
}

export const AgentList: React.FC<AgentListProps> = ({ onSelectAgent }) => {
  return (
    <div className="space-y-4">
      <h2 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
        Your Agents
      </h2>
      <div className="space-y-3">
        {dummyAgents.map((agent) => (
          <AgentCard
            key={agent.id}
            {...agent}
            onSelect={onSelectAgent}
          />
        ))}
      </div>
    </div>
  );
}; 