export type ProviderType = 'Collection' | 'Prompts' | 'Semantic Kernel Plugin';

export interface Plugin {
  id: string;
  name: string;
  description: string;
  provider: ProviderType;
  created_date?: string;
}

export interface PluginFormData {
  name: string;
  description: string;
  provider: ProviderType;
}
