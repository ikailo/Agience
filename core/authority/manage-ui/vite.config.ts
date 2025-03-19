import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import dotenv from 'dotenv';
import dotenvExpand from 'dotenv-expand';

const env = dotenv.config({ path: path.resolve(__dirname, '../.env') });
dotenvExpand.expand(env);

  // Filter only public variables (those starting with VITE_)
const publicEnv = Object.keys(process.env).reduce((acc, key) => {
  if (key.startsWith('VITE_')) {
    acc[key] = process.env[key] as string;
  }
  return acc;
}, {} as Record<string, string>);

let httpsConfig;
const keyPath = process.env.WAN_KEY_PATH;
const certPath = process.env.WAN_CRT_PATH;
if (keyPath && certPath) {
  httpsConfig = {
    key: fs.readFileSync(path.resolve(__dirname, "../", keyPath)),
    cert: fs.readFileSync(path.resolve(__dirname, "../", certPath))
  };
}

export default defineConfig({
  plugins: [react()],
  envDir: '../', // Explicitly set the env file directory
  server: {
    ...(httpsConfig ? { https: httpsConfig } : {}),
    host: "localhost",
    port: 5002
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@layouts': path.resolve(__dirname, './src/layouts'),
      '@assets': path.resolve(__dirname, './src/assets'),
      '@utils': path.resolve(__dirname, './src/utils')
    }
  },
  // Inject only the public, expanded environment variables into the client bundle
  define: {
    'import.meta.env': JSON.stringify(publicEnv)
  }
});
