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
  executiveFunctionId: string | null;
  autoStartFunctionId: string | null;
  onAutoStartFunctionComplete: number | null;
  hostId: string | null;
  owner_id: string;
  owner: Owner;
  created_date: string;
  executiveFunction: any | null;
  autoStartFunction: any | null;
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
  hostId: string | null;
  executiveFunctionId: string | null;
  is_enabled: boolean;
  image?: File;
} 