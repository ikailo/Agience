import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Topic, TopicFormData } from '../../types/Topic';
import { topicService } from '../../services/api/topicService';
//import TopicTable from './TopicTable';
import TopicForm from './TopicForm';
import ConfirmationModal from '../common/ConfirmationModal';
import NotificationModal from '../common/NotificationModal';
import Card from '../common/Card';

/**
 * TopicDetails component that displays and manages topics
 */
export const TopicDetails = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [topics, setTopics] = useState<Topic[]>([]);
  const [selectedTopic, setSelectedTopic] = useState<Topic | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [showNotification, setShowNotification] = useState(false);
  const [notificationMessage, setNotificationMessage] = useState({ title: '', message: '', type: 'info' as 'success' | 'error' | 'info' | 'warning' });
  const [topicToDelete, setTopicToDelete] = useState<Topic | null>(null);
  const [formData, setFormData] = useState<TopicFormData>({
    name: '',
    description: ''
  });

  // Get topic ID from URL if available
  const topicId = searchParams.get('id');

  // Fetch topics
  const fetchTopics = useCallback(async () => {
    try {
      setIsLoading(true);
      const topicsData = await topicService.getAllTopics();
      setTopics(topicsData);

      // If there's a topic ID in the URL, select that topic
      if (topicId) {
        const topic = topicsData.find(t => t.id === topicId);
        if (topic) {
          setSelectedTopic(topic);
        }
      }
    } catch (error) {
      console.error('Error fetching topics:', error);
      showNotificationModal('Error', 'Failed to fetch topics', 'error');
    } finally {
      setIsLoading(false);
    }
  }, [topicId]);

  // Initial data fetch
  useEffect(() => {
    fetchTopics();
  }, [fetchTopics]);

  // Handle topic selection
  const handleSelectTopic = (topic: Topic) => {
    setSelectedTopic(topic);
    setSearchParams({ id: topic.id });
  };

  // Handle add topic
  const handleAddTopic = () => {
    setFormData({ name: '', description: '' });
    setSelectedTopic(null);
    setShowForm(true);
    setSearchParams({});
  };

  // Handle edit topic
  const handleEditTopic = (topic: Topic) => {
    setFormData({
      name: topic.name,
      description: topic.description
    });
    setSelectedTopic(topic);
    setShowForm(true);
  };

  // Handle delete click
  const handleDeleteClick = (topic: Topic) => {
    setTopicToDelete(topic);
    setShowConfirmation(true);
  };

  // Handle delete confirmation
  const handleDeleteTopic = async () => {
    if (!topicToDelete) return;

    try {
      await topicService.deleteTopic(topicToDelete.id);
      await fetchTopics();
      setShowConfirmation(false);
      setTopicToDelete(null);
      setSelectedTopic(null);
      setSearchParams({});
      showNotificationModal('Success', 'Topic deleted successfully', 'success');
    } catch (error) {
      console.error('Error deleting topic:', error);
      showNotificationModal('Error', 'Failed to delete topic', 'error');
    }
  };

  // Handle save topic
  const handleSaveTopic = async (formData: TopicFormData): Promise<void> => {
    try {
      if (selectedTopic) {
        // Update existing topic
        await topicService.updateTopic(selectedTopic.id, formData);
        showNotificationModal('Success', 'Topic updated successfully', 'success');
      } else {
        // Create new topic
        const newTopic = await topicService.createTopic(formData);
        showNotificationModal('Success', 'Topic created successfully', 'success');
        // Select the newly created topic
        setSelectedTopic(newTopic);
        setSearchParams({ id: newTopic.id });
      }
      setShowForm(false);
      await fetchTopics();
    } catch (error) {
      console.error('Error saving topic:', error);
      showNotificationModal('Error', 'Failed to save topic', 'error');
    }
  };

  // Show notification modal
  const showNotificationModal = (title: string, message: string, type: 'success' | 'error' | 'info' | 'warning') => {
    setNotificationMessage({ title, message, type });
    setShowNotification(true);
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
      {/* Topics List */}
      <div className="lg:col-span-1">
        <Card
          title="My Topics"
          actions={
            <button
              onClick={handleAddTopic}
              className="px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors flex items-center space-x-1 text-sm font-medium shadow-sm"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              <span>New Topic</span>
            </button>
          }
        >
          <div className="space-y-3 max-h-[calc(100vh-250px)] overflow-y-auto pr-1">
            {isLoading ? (
              <div className="flex justify-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
              </div>
            ) : topics.length === 0 ? (
              <div className="text-center py-8 px-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <p className="text-gray-600 dark:text-gray-400">No topics found</p>
                <button
                  onClick={handleAddTopic}
                  className="mt-3 px-4 py-2 bg-indigo-600 text-white text-sm rounded-md hover:bg-indigo-700 transition-colors"
                >
                  Create your first topic
                </button>
              </div>
            ) : (
              topics.map(topic => (
                <div
                  key={topic.id}
                  className={`
                    p-3 rounded-lg cursor-pointer transition-colors
                    ${selectedTopic?.id === topic.id
                      ? 'bg-indigo-50 dark:bg-indigo-900 border-l-4 border-indigo-500'
                      : 'bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 border-l-4 border-transparent'
                    }
                  `}
                  onClick={() => handleSelectTopic(topic)}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="font-medium text-gray-900 dark:text-white">{topic.name}</h3>
                      <p className="text-sm text-gray-500 dark:text-gray-400 mt-1 line-clamp-2">
                        {topic.description || 'No description'}
                      </p>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </Card>
      </div>

      {/* Topic Details/Form */}
      <div className="lg:col-span-3">
        {showForm ? (
          <TopicForm
            initialData={formData}
            onSubmit={handleSaveTopic}
            onCancel={() => setShowForm(false)}
            isLoading={isLoading}
          />
        ) : selectedTopic ? (
          <Card
            title={`Topic Details: ${selectedTopic.name}`}
            actions={
              <div className="flex space-x-2">
                <button
                  onClick={() => handleEditTopic(selectedTopic)}
                  className="px-3 py-1.5 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors text-sm font-medium shadow-sm"
                >
                  Edit
                </button>
                <button
                  onClick={() => handleDeleteClick(selectedTopic)}
                  className="px-3 py-1.5 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors text-sm font-medium shadow-sm"
                >
                  Delete
                </button>
              </div>
            }
          >
            <div className="space-y-4">
              <div>
                <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300">Description</h4>
                <p className="mt-1 text-gray-600 dark:text-gray-400">
                  {selectedTopic.description || 'No description provided'}
                </p>
              </div>
              {selectedTopic.created_date && (
                <div>
                  <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300">Created</h4>
                  <p className="mt-1 text-gray-600 dark:text-gray-400">
                    {new Date(selectedTopic.created_date).toLocaleDateString()}
                  </p>
                </div>
              )}
            </div>
          </Card>
        ) : (
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 text-center shadow-lg">
            <p className="text-gray-600 dark:text-gray-300">
              Select a topic from the list or create a new one to get started.
            </p>
          </div>
        )}
      </div>

      {/* Confirmation Modal */}
      <ConfirmationModal
        isOpen={showConfirmation}
        onClose={() => setShowConfirmation(false)}
        onConfirm={handleDeleteTopic}
        title="Delete Topic"
        message={`Are you sure you want to delete the topic "${topicToDelete?.name}"? This action cannot be undone.`}
        confirmText="Delete"
        type="danger"
      />

      {/* Notification Modal */}
      <NotificationModal
        isOpen={showNotification}
        onClose={() => setShowNotification(false)}
        title={notificationMessage.title}
        message={notificationMessage.message}
        type={notificationMessage.type}
      />
    </div>
  );
}; 