// This file contains no secrets, just the structure
const requiredEnvVars = [
  'VITE_AUTHORITY_PUBLIC_URI',
  'VITE_MANAGE_UI_CLIENT_ID',
  'VITE_MANAGE_UI_REDIRECT_URI',
] as const;

// Check for missing environment variables
requiredEnvVars.forEach((envVar) => {
  if (!import.meta.env[envVar]) {
    throw new Error(`Missing required environment variable: ${envVar}`);
  }
});

// Export environment variables with types
export const env = {
  OIDC_AUTHORITY: import.meta.env.VITE_AUTHORITY_PUBLIC_URI,
  OIDC_CLIENT_ID: import.meta.env.VITE_MANAGE_UI_CLIENT_ID,
  OIDC_REDIRECT_URI: import.meta.env.VITE_MANAGE_UI_REDIRECT_URI,
  OIDC_POST_LOGOUT_REDIRECT_URI: import.meta.env.VITE_MANAGE_UI_LOGOUT_REDIRECT_URI,
  OIDC_AUTHORIZATION_ENDPOINT: import.meta.env.VITE_AUTHORITY_AUTHORIZATION_ENDPOINT,
  OIDC_TOKEN_ENDPOINT: import.meta.env.VITE_AUTHORITY_TOKEN_ENDPOINT,
  OIDC_USERINFO_ENDPOINT: import.meta.env.VITE_AUTHORITY_USERINFO_ENDPOINT,
  OIDC_REVOCATION_ENDPOINT: import.meta.env.VITE_AUTHORITY_REVOCATION_ENDPOINT,
} as const; 