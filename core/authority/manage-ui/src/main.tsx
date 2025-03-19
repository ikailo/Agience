import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

 //console.log('Testing ENV:', {
 //  VITE_AUTHORITY_PUBLIC_URI: import.meta.env.VITE_AUTHORITY_PUBLIC_URI,
 //  MODE: import.meta.env.MODE,
 // });

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
