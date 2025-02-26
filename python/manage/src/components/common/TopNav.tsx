import { useTheme } from '../../hooks/useTheme';
import { UserProfile } from './UserProfile';
import { useAuth } from '../../auth/AuthContext';

interface TopNavProps {
  onToggleSidebar: () => void;
}

export const TopNav: React.FC<TopNavProps> = ({ onToggleSidebar }) => {
  const { theme, toggleTheme } = useTheme();
  const { logout } = useAuth();

  return (
    <header className="fixed top-0 left-0 right-0 bg-gray-50 dark:bg-gray-800 shadow z-10">
      <nav className="h-16 flex items-center py-8">
        {/* Hamburger Menu */}
        <button
          onClick={onToggleSidebar}
          className="p-4 pl-4 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
          aria-label="Toggle sidebar"
        >
          <svg
            className="h-6 w-6 text-gray-600 dark:text-gray-300"
            fill="none"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth="2"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>

        {/* Logo and Company Name */}
        <div className="flex items-center space-x-3 ml-4">
          <img src="/logo.png" alt="Agience Logo" className="h-8 w-8" />
          <h1 className="text-xl font-bold text-gray-900 dark:text-white hidden sm:block">Agience</h1>
        </div>

        {/* Right side - User Profile and Theme Toggle */}
        <div className="flex items-center space-x-4 ml-auto px-4 sm:px-6 lg:px-8">
          <button
            onClick={toggleTheme}
            className="p-2 rounded-lg bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white"
            aria-label="Toggle theme"
          >
            {theme === 'dark' ? '☀️' : '🌙'}
          </button>
          <UserProfile name="John Doe" />
          <button
            onClick={() => logout()}
            className="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
          >
            Sign Out
          </button>
        </div>
      </nav>
    </header>
  );
}; 