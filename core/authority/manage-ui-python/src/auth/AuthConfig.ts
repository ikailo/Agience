import { env } from '../config/env';

// Debugging for OAuth configuration and redirect URI
console.log('Redirect URI:', env.OIDC_REDIRECT_URI);

// Add comprehensive debugging
console.log('OAuth Configuration:', {
  authority: env.OIDC_AUTHORITY,
  client_id: env.OIDC_CLIENT_ID,
  redirect_uri: env.OIDC_REDIRECT_URI,
  authorization_endpoint: env.OIDC_AUTHORIZATION_ENDPOINT
});

export const AuthConfig = {
  authority: env.OIDC_AUTHORITY,
  client_id: env.OIDC_CLIENT_ID,
//  client_secret: env.OIDC_CLIENT_SECRET,
  redirect_uri: env.OIDC_REDIRECT_URI,
  post_logout_redirect_uri: env.OIDC_POST_LOGOUT_REDIRECT_URI,
  scope: 'openid profile email manage',
  response_type: 'code',
  loadUserInfo: true,
  automaticSilentRenew: true,
  monitorSession: true,
  revokeTokensOnSignout: true,
  metadata: {
    authorization_endpoint: env.OIDC_AUTHORIZATION_ENDPOINT,
    token_endpoint: env.OIDC_TOKEN_ENDPOINT,
    userinfo_endpoint: env.OIDC_USERINFO_ENDPOINT,
    revocation_endpoint: env.OIDC_REVOCATION_ENDPOINT
  },
  extraTokenParameters: {
    client_id: env.OIDC_CLIENT_ID
  },
  extraQueryParams: {
    access_type: 'offline'//,
    //prompt: 'consent'
  }
}; 