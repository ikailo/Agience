import { Authorizer } from './Authorizer';

export interface Connection {
  id: string;
  name: string;
  description: string;
  authorizer_id: string;
  authorizer?: Authorizer;
  created_date?: string;
}

export interface ConnectionFormData {
  name: string;
  description: string;
  authorizer_id: string;
} 