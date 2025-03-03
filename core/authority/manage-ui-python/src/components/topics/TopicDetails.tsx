import { useState, useEffect } from 'react';
import { Button } from '../common/Button';
import { TopicTable } from './TopicTable';
import { TopicModal } from './TopicModal';
import { topicService } from '../../services/api/topicService';
import { Topic, TopicFormData } from '../../types/Topic';

export const TopicDetails = () => {
  const [topics, setTopics] = useState<Topic[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedTopic, setSelectedTopic] = useState<Topic | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  
  // Delete confirmation state
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
  const [topicToDelete, setTopicToDelete] = useState<Topic | null>(null);

  // Fetch topics when component mounts
  useEffect(() => {
    fetchTopics();
  }, []);

  // Function to fetch all topics
  const fetchTopics = async () => {
    try {
      setIsLoading(true);
      const data = await topicService.getAllTopics();
      setTopics(data);
      setError(null);
    } catch (err) {
      console.error('Error fetching topics:', err);
      setError('Failed to load topics. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  // Function to handle adding a new topic
  const handleAddTopic = () => {
    setSelectedTopic(null);
    setIsEditing(false);
    setIsModalOpen(true);
  };

  // Function to handle editing a topic
  const handleEditTopic = (topic: Topic) => {
    setSelectedTopic(topic);
    setIsEditing(true);
    setIsModalOpen(true);
  };

  // Function to handle deleting a topic
  const handleDeleteClick = (topic: Topic) => {
    setTopicToDelete(topic);
    setIsDeleteConfirmOpen(true);
  };

  // Function to confirm and execute topic deletion
  const handleDeleteTopic = async () => {
    if (!topicToDelete) return;
    
    try {
      await topicService.deleteTopic(topicToDelete.id);
      
      // Show success message
      setSuccessMessage('Topic deleted successfully!');
      
      // Refresh the topics list
      fetchTopics();
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
    } catch (err) {
      console.error('Error deleting topic:', err);
      setError('Failed to delete topic. Please try again.');
    } finally {
      setIsDeleteConfirmOpen(false);
      setTopicToDelete(null);
    }
  };

  // Function to handle saving a topic (create or update)
  const handleSaveTopic = async (formData: TopicFormData): Promise<void> => {
    try {
      // If editing, update existing topic
      if (isEditing && selectedTopic) {
        await topicService.updateTopic(selectedTopic.id, formData);
        setSuccessMessage('Topic updated successfully!');
      } else {
        // Otherwise create new topic
        await topicService.createTopic(formData);
        setSuccessMessage('Topic created successfully!');
      }
      
      // Refresh the topics list
      fetchTopics();
      
      // Close the modal
      setIsModalOpen(false);
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
    } catch (err) {
      console.error('Error saving topic:', err);
      setError('Failed to save topic. Please try again.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Topics
        </h2>
        <Button
          variant="primary"
          onClick={handleAddTopic}
          disabled={isLoading}
        >
          {isLoading ? (
            <span className="flex items-center">
              <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Processing...
            </span>
          ) : (
            'Add Topic'
          )}
        </Button>
      </div>

      {/* Success message */}
      {successMessage && (
        <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-4 animate-fadeIn">
          <p className="text-green-600 dark:text-green-400">{successMessage}</p>
        </div>
      )}

      {/* Error message */}
      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4">
          <p className="text-red-600 dark:text-red-400">{error}</p>
        </div>
      )}

      {/* Topic Table */}
      <TopicTable 
        topics={topics} 
        onEdit={handleEditTopic} 
        onDelete={handleDeleteClick} 
        isLoading={isLoading}
      />

      {/* Topic Modal */}
      <TopicModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSaveTopic}
        initialData={selectedTopic ? {
          name: selectedTopic.name,
          description: selectedTopic.description,
        } : undefined}
        isEditing={isEditing}
      />

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && topicToDelete && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md">
            <div className="p-6">
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">
                Confirm Delete
              </h3>
              <p className="text-gray-500 dark:text-gray-400 mb-6">
                Are you sure you want to delete the topic "{topicToDelete.name}"? This action cannot be undone.
              </p>
              <div className="flex justify-end space-x-3">
                <Button
                  variant="secondary"
                  onClick={() => setIsDeleteConfirmOpen(false)}
                >
                  Cancel
                </Button>
                <Button
                  variant="danger"
                  onClick={handleDeleteTopic}
                >
                  Delete
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}; 