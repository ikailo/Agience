import { useState } from 'react';
import { TopNav } from '../common/TopNav';
import { SideNav } from '../common/SideNav';

interface LayoutProps {
  children: React.ReactNode;
}

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);

  return (
    <div className="min-h-screen bg-white dark:bg-gray-900 transition-colors">
      <TopNav onToggleSidebar={() => setIsSidebarOpen(!isSidebarOpen)} />
      <SideNav isOpen={isSidebarOpen} />

      {/* Main Content */}
      <main className={`pt-16 transition-all duration-300 ease-in-out
        ${isSidebarOpen ? 'sm:ml-64 ml-0' : 'sm:ml-16 ml-0'} 
        ${isSidebarOpen ? 'pl-0' : 'pl-16 sm:pl-0'}`}
      >
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 text-gray-900 dark:text-white">
          {children}
        </div>
      </main>
    </div>
  );
}; 