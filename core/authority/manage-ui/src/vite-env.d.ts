/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_MANAGE_UI_ORIGIN_URI: string
  readonly VITE_AUTHORITY_PUBLIC_URI: string
  readonly VITE_MANAGE_UI_CLIENT_ID: string
  readonly VITE_MANAGE_UI_REDIRECT_URI: string
  readonly VITE_MANAGE_UI_LOGOUT_REDIRECT_URI: string
  readonly VITE_OIDC_SCOPE: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
