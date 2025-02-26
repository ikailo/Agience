// This file contains no secrets, just the structure
const requiredEnvVars = [
  'VITE_OIDC_AUTHORITY',
  'VITE_OIDC_CLIENT_ID',
  'VITE_OIDC_CLIENT_SECRET',
  'VITE_OIDC_REDIRECT_URI',
] as const;

// Check for missing environment variables
requiredEnvVars.forEach((envVar) => {
  if (!import.meta.env[envVar]) {
    throw new Error(`Missing required environment variable: ${envVar}`);
  }
});

// Export environment variables with types
export const env = {
  OIDC_AUTHORITY: import.meta.env.VITE_OIDC_AUTHORITY,
  OIDC_CLIENT_ID: import.meta.env.VITE_OIDC_CLIENT_ID,
  OIDC_CLIENT_SECRET: import.meta.env.VITE_OIDC_CLIENT_SECRET,
  OIDC_REDIRECT_URI: import.meta.env.VITE_OIDC_REDIRECT_URI,
  OIDC_POST_LOGOUT_REDIRECT_URI: import.meta.env.VITE_OIDC_POST_LOGOUT_REDIRECT_URI,
  OIDC_AUTHORIZATION_ENDPOINT: import.meta.env.VITE_OIDC_AUTHORIZATION_ENDPOINT,
  OIDC_TOKEN_ENDPOINT: import.meta.env.VITE_OIDC_TOKEN_ENDPOINT,
  OIDC_USERINFO_ENDPOINT: import.meta.env.VITE_OIDC_USERINFO_ENDPOINT,
  OIDC_REVOCATION_ENDPOINT: import.meta.env.VITE_OIDC_REVOCATION_ENDPOINT,
} as const; 