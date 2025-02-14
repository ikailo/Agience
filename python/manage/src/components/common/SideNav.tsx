import { Link, useLocation } from 'react-router-dom';
import { navItems } from '../../config/navigation';


interface SideNavProps {
  isOpen: boolean;
}

export const SideNav: React.FC<SideNavProps> = ({ isOpen }) => {
  const location = useLocation();

  return (
    <aside 
      className={`fixed left-0 top-16 h-[calc(100vh-4rem)] bg-gray-50 dark:bg-gray-800 
        transition-all duration-300 ease-in-out border-r border-gray-200 dark:border-gray-700
        ${isOpen ? 'w-64' : 'w-0 -translate-x-full sm:translate-x-0 sm:w-16'}`}
    >
      <nav className="h-full py-4">
        <ul className="space-y-2">
          {navItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <li key={item.path}>
                <Link
                  to={item.path}
                  className={`flex items-center px-4 py-2 transition-colors relative
                    ${isActive 
                      ? 'sm:bg-blue-50 sm:dark:bg-blue-900/20 text-blue-600 dark:text-blue-400' 
                      : 'text-gray-700 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-700'
                    }`}
                >
                  {/* Active indicator bar */}
                  {isActive && (
                    <div className="absolute left-0 top-0 h-full w-1 bg-blue-600 dark:bg-blue-400" />
                  )}
                  
                  {/* Icon with active state */}
                  <img 
                    src={item.icon} 
                    alt={`${item.label} icon`}
                    className={`w-6 h-6 ${isActive ? 'brightness-[0.4] dark:filter dark:brightness-0 dark:invert' : ''}`}
                  />
                  
                  {/* Label */}
                  <span className={`ml-3 ${!isOpen ? 'hidden' : 'block'}`}>
                    {item.label}
                  </span>
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>
    </aside>
  );
}; 