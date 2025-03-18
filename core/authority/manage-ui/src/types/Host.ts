export interface Host {
  id: string;
  name: string;
  description: string;
  operatorId?: string; // Optional field
  created_date: string;
  scopes: string[]
}

export interface HostFormData {
  name: string;
  description: string;
  operatorId?: string;
  scopes: string[]
} 