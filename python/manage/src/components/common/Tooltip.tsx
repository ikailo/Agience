import { useState, useRef, useEffect } from 'react';

interface TooltipProps {
  text: string;
  children: React.ReactNode;
  position?: 'top' | 'right' | 'bottom' | 'left';
  width?: string;
}

export const Tooltip: React.FC<TooltipProps> = ({ 
  text, 
  children, 
  position = 'top',
  width = 'w-64'
}) => {
  const [isVisible, setIsVisible] = useState(false);
  const tooltipRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLDivElement>(null);

  // Handle click outside to close tooltip
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        tooltipRef.current && 
        triggerRef.current && 
        !tooltipRef.current.contains(event.target as Node) && 
        !triggerRef.current.contains(event.target as Node)
      ) {
        setIsVisible(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Position classes based on the position prop
  const positionClasses = {
    top: 'bottom-full left-1/2 transform -translate-x-1/2 mb-2',
    right: 'left-full top-1/2 transform -translate-y-1/2 ml-2',
    bottom: 'top-full left-1/2 transform -translate-x-1/2 mt-2',
    left: 'right-full top-1/2 transform -translate-y-1/2 mr-2'
  };

  // Arrow classes based on the position prop
  const arrowClasses = {
    top: 'top-full left-1/2 transform -translate-x-1/2 border-t-gray-800 dark:border-t-gray-700 border-l-transparent border-r-transparent border-b-transparent',
    right: 'right-full top-1/2 transform -translate-y-1/2 border-r-gray-800 dark:border-r-gray-700 border-t-transparent border-b-transparent border-l-transparent',
    bottom: 'bottom-full left-1/2 transform -translate-x-1/2 border-b-gray-800 dark:border-b-gray-700 border-l-transparent border-r-transparent border-t-transparent',
    left: 'left-full top-1/2 transform -translate-y-1/2 border-l-gray-800 dark:border-l-gray-700 border-t-transparent border-b-transparent border-r-transparent'
  };

  return (
    <div className="relative inline-flex items-center">
      <div 
        ref={triggerRef}
        className="inline-flex items-center cursor-help"
        onMouseEnter={() => setIsVisible(true)}
        onMouseLeave={() => setIsVisible(false)}
        onClick={() => setIsVisible(!isVisible)}
      >
        {children}
      </div>
      
      {isVisible && (
        <div 
          ref={tooltipRef}
          className={`absolute z-50 ${positionClasses[position]} ${width} bg-gray-800 dark:bg-gray-700 text-white p-3 rounded-md shadow-lg text-sm`}
          role="tooltip"
        >
          <div className={`absolute w-0 h-0 border-4 ${arrowClasses[position]}`}></div>
          <div className="relative z-10">{text}</div>
        </div>
      )}
    </div>
  );
}; 