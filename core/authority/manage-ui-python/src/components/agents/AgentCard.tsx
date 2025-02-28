interface AgentCardProps {
  id: string;
  name: string;
  description: string;
  imageUrl: string;
  onSelect: (id: string) => void;
}

export const AgentCard: React.FC<AgentCardProps> = ({
  id,
  name,
  description,
  imageUrl,
  onSelect,
}) => {
  return (
    <div 
      onClick={() => onSelect(id)}
      className="flex items-start space-x-4 p-4 rounded-lg bg-gray-50 dark:bg-gray-800 
        border border-gray-100 dark:border-gray-700
        shadow-sm hover:shadow-md transition-all duration-200 
        hover:bg-white dark:hover:bg-gray-700 cursor-pointer"
    >
      <img
        src={imageUrl}
        alt={`${name} avatar`}
        className="w-12 h-12 rounded-full object-cover flex-shrink-0"
      />
      <div className="flex-1 min-w-0">
        <h3 className="text-sm font-medium text-gray-900 dark:text-white truncate">
          {name}
        </h3>
        <p className="text-sm text-gray-500 dark:text-gray-400 line-clamp-2">
          {description}
        </p>
      </div>
    </div>
  );
}; 