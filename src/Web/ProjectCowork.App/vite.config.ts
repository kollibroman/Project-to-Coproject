import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { heyApiPlugin } from '@hey-api/vite-plugin';

// https://vite.dev/config/
export default defineConfig({
  plugins: [heyApiPlugin({
    config: {
      input: '../ProjectCowork.Api/project-cowork.json',
      output: 'src/client',
      plugins: [
        '@hey-api/client-fetch',
        '@tanstack/react-query'
      ]
    },}),
    react()],
})
