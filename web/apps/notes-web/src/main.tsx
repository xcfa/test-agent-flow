import { createNotesClient } from '@xcfa/notes-api-client';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import './styles.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App client={createNotesClient()} />
  </StrictMode>,
);
