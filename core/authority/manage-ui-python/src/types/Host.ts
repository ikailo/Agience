export interface Host {
  id: string;
  name: string;
  address: string;
  operatorId?: string; // Optional field
  created_date: string;
}

export interface HostFormData {
  name: string;
  address: string;
  operatorId?: string;
} 