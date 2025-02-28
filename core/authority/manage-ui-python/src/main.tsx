import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

// console.log('Testing ENV:', {
//   VITE_OIDC_AUTHORITY: import.meta.env.VITE_OIDC_AUTHORITY,
//   MODE: import.meta.env.MODE,
// });

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
