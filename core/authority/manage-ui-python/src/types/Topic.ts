export interface Topic {
  id: string;
  name: string;
  description: string;
  created_date?: string;
}

export interface TopicFormData {
  name: string;
  description: string;
} 