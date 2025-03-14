export type AuthorizerType = 'API Key' | 'Public' | 'OAuth';

export interface Authorizer {
  id: string;
  name: string;
  type: AuthorizerType;
  created_date?: string;
}

export interface AuthorizerFormData {
  name: string;
  type: AuthorizerType;
} 