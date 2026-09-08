import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import './media/generatedArt';
import './styles/global.css';

const container = document.getElementById('root');
if (!container) throw new Error('Missing #root');

createRoot(container).render(
  <StrictMode>
    <App />
  </StrictMode>,
);

// The service worker only caches this app's own files, so the game keeps
// working offline. It never contacts a third party.
if ('serviceWorker' in navigator && import.meta.env.PROD) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js').catch(() => {
      /* offline support is optional; a failure must not break play */
    });
  });
}
