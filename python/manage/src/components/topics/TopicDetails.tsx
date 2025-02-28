import { useState } from 'react';
import { Button } from '../common/Button';
import { TopicTable } from './TopicTable';

interface Topic {
  id: string;
  name: string;
  description: string;
  address: string;
}

export const TopicDetails = () => {
  const [topics, setTopics] = useState<Topic[]>([
    {
      id: '1',
      name: 'Weather Updates',
      description: 'Real-time weather information',
      address: '/topics/weather',
    },
    // Add more sample data as needed
  ]);

  const handleEdit = (id: string, updatedTopic: Topic) => {
    setTopics(topics.map(topic => 
      topic.id === id ? updatedTopic : topic
    ));
  };

  const handleDelete = (id: string) => {
    setTopics(topics.filter(topic => topic.id !== id));
  };

  const handleAdd = () => {
    const newTopic: Topic = {
      id: Date.now().toString(),
      name: '',
      description: '',
      address: '',
    };
    setTopics([...topics, newTopic]);
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          Topics
        </h2>
        <Button
          variant="primary"
          onClick={handleAdd}
        >
          Add Topic
        </Button>
      </div>
      
      <TopicTable
        topics={topics}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />
    </div>
  );
}; 