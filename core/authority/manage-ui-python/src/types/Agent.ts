export interface Owner {
  name: string;
  id: string;
  created_date: string;
}

export interface Agent {
  id: string;
  name: string;
  description: string;
  persona: string | null;
  is_enabled: boolean;
  executive_function_id: string | null;
  auto_start_function_id: string | null;
  on_auto_start_function_complete: number | null;
  host_id: string | null;
  owner_id: string;
  owner: Owner;
  created_date: string;
  executive_function: any | null;
  auto_start_function: any | null;
  host: any | null;
  is_connected: boolean;
  topics: any[];
  plugins: any[];
  imageUrl?: string; // Optional field for UI display
}

export interface AgentFormData {
  name: string;
  description: string;
  persona: string | null;
  host_id: string | null;
  executive_function_id: string | null;
  is_enabled: boolean;
  image?: File;
} 