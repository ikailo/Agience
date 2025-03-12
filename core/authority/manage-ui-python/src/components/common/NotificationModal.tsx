import React from 'react';

interface NotificationModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
}

/**
 * A reusable notification modal component for displaying messages
 */
const NotificationModal: React.FC<NotificationModalProps> = ({
  isOpen,
  onClose,
  title,
  message,
  type
}) => {
  if (!isOpen) return null;

  const getTypeStyles = () => {
    switch (type) {
      case 'success':
        return {
          icon: (
            <svg className="h-6 w-6 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          ),
          bgColor: 'bg-green-50 dark:bg-green-900/20',
          borderColor: 'border-green-200 dark:border-green-800',
          textColor: 'text-green-800 dark:text-green-300',
          buttonBgColor: 'bg-green-600 hover:bg-green-700 dark:bg-green-700 dark:hover:bg-green-600'
        };
      case 'error':
        return {
          icon: (
            <svg className="h-6 w-6 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          ),
          bgColor: 'bg-red-50 dark:bg-red-900/20',
          borderColor: 'border-red-200 dark:border-red-800',
          textColor: 'text-red-800 dark:text-red-300',
          buttonBgColor: 'bg-red-600 hover:bg-red-700 dark:bg-red-700 dark:hover:bg-red-600'
        };
      case 'warning':
        return {
          icon: (
            <svg className="h-6 w-6 text-yellow-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          ),
          bgColor: 'bg-yellow-50 dark:bg-yellow-900/20',
          borderColor: 'border-yellow-200 dark:border-yellow-800',
          textColor: 'text-yellow-800 dark:text-yellow-300',
          buttonBgColor: 'bg-yellow-600 hover:bg-yellow-700 dark:bg-yellow-700 dark:hover:bg-yellow-600'
        };
      case 'info':
      default:
        return {
          icon: (
            <svg className="h-6 w-6 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          ),
          bgColor: 'bg-blue-50 dark:bg-blue-900/20',
          borderColor: 'border-blue-200 dark:border-blue-800',
          textColor: 'text-blue-800 dark:text-blue-300',
          buttonBgColor: 'bg-blue-600 hover:bg-blue-700 dark:bg-blue-700 dark:hover:bg-blue-600'
        };
    }
  };

  const styles = getTypeStyles();

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50 transition-opacity duration-300">
      <div className={`max-w-md w-full rounded-lg shadow-xl overflow-hidden transform transition-all duration-300 ${styles.bgColor} border bg-opacity-0 ${styles.borderColor}`}>
        <div className="p-6">
          <div className="flex items-start mb-4">
            <div className="flex-shrink-0">
              {styles.icon}
            </div>
            <div className="ml-3 flex-1">
              <h3 className={`text-lg font-medium ${styles.textColor}`}>
                {title}
              </h3>
              <div className={`mt-2 ${styles.textColor} text-sm`}>
                <p>{message}</p>
              </div>
            </div>
          </div>
          <div className="mt-5 flex justify-end">
            <button
              type="button"
              onClick={onClose}
              className={`inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 text-sm font-medium text-white focus:outline-none focus:ring-2 focus:ring-offset-2 ${styles.buttonBgColor}`}
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default NotificationModal; 