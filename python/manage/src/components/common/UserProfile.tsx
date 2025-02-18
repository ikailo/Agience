import { getInitials } from '../../utils/initials';

interface UserProfileProps {
  name: string;
}

export const UserProfile: React.FC<UserProfileProps> = ({ name }) => {
  return (
    <div className="flex items-center space-x-3">
      <span className="text-gray-900 dark:text-white hidden sm:block">{name}</span>
      <div className="h-8 w-8 rounded-full bg-blue-500 flex items-center justify-center text-white">
        <span className="text-sm font-medium">{getInitials(name)}</span>
      </div>
    </div>
  );
}; 