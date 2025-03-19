import { apiClient } from './config';
import { Plugin } from '../../types/Plugin';

export interface HostPlugin {
  id: string;
  hostId: string;
  pluginId: string;
  plugin?: Plugin;
  created_date?: string;
}

export const hostPluginService = {
  getPluginsForHost: async (hostId: string): Promise<Plugin[]> => {
    try {
      const response = await apiClient.get<Plugin[]>(`/manage/host/${hostId}/plugins`);
      return Array.isArray(response.data) ? response.data : [];
    } catch (error) {
      console.error(`Error fetching plugins for host ${hostId}:`, error);
      throw error;
    }
  },

  addPluginToHost: async (hostId: string, pluginId: string): Promise<HostPlugin> => {
    try {
      const response = await apiClient.post<HostPlugin>(`/manage/host/${hostId}/plugin/${pluginId}`);
      return response.data;
    } catch (error) {
      console.error('Error adding plugin to host:', error);
      throw error;
    }
  },

  removePluginFromHost: async (hostId: string, pluginId: string): Promise<void> => {
    try {
      await apiClient.delete(`/manage/host/${hostId}/plugin/${pluginId}`);
    } catch (error) {
      console.error('Error removing plugin from host:', error);
      throw error;
    }
  }
}; 