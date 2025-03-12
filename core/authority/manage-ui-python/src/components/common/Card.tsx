import React, { ReactNode } from 'react';

interface CardProps {
  title?: string | ReactNode;
  subtitle?: string;
  children: ReactNode;
  footer?: ReactNode;
  className?: string;
  headerClassName?: string;
  bodyClassName?: string;
  footerClassName?: string;
  variant?: 'default' | 'bordered' | 'flat';
  actions?: ReactNode;
}

/**
 * A reusable card component with different variants
 */
const Card: React.FC<CardProps> = ({
  title,
  subtitle,
  children,
  footer,
  className = '',
  headerClassName = '',
  bodyClassName = '',
  footerClassName = '',
  variant = 'default',
  actions
}) => {
  // Base classes
  const baseClasses = 'overflow-hidden transition-shadow duration-300';
  
  // Variant classes
  const variantClasses = {
    default: 'bg-white dark:bg-gray-800 rounded-lg shadow-lg',
    bordered: 'bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700',
    flat: 'bg-white dark:bg-gray-800 rounded-lg'
  };
  
  // Combine classes
  const cardClasses = `${baseClasses} ${variantClasses[variant]} ${className}`;
  
  return (
    <div className={cardClasses}>
      {/* Card Header */}
      {(title || actions) && (
        <div className={`px-6 py-4 flex justify-between items-center border-b border-gray-200 dark:border-gray-700 ${headerClassName}`}>
          <div>
            {typeof title === 'string' ? (
              <h3 className="text-lg font-medium text-gray-900 dark:text-white">
                {title}
              </h3>
            ) : (
              title
            )}
            
            {subtitle && (
              <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                {subtitle}
              </p>
            )}
          </div>
          
          {actions && (
            <div className="flex items-center space-x-2">
              {actions}
            </div>
          )}
        </div>
      )}
      
      {/* Card Body */}
      <div className={`p-6 ${bodyClassName}`}>
        {children}
      </div>
      
      {/* Card Footer */}
      {footer && (
        <div className={`px-6 py-4 bg-gray-50 dark:bg-gray-700 border-t border-gray-200 dark:border-gray-700 ${footerClassName}`}>
          {footer}
        </div>
      )}
    </div>
  );
};

export default Card; 