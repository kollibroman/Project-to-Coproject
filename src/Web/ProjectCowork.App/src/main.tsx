import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import '@mantine/core/styles.css'
import './index.css'
import {createTheme, MantineProvider} from '@mantine/core'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import App from './App.tsx'

const theme = createTheme({
    defaultRadius: "lg",
});

const queryClient = new QueryClient();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <MantineProvider theme={theme} defaultColorScheme="dark">
        <App />
      </MantineProvider>
    </QueryClientProvider>
  </StrictMode>,
)
