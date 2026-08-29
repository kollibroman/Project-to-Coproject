import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { heyApiPlugin } from '@hey-api/vite-plugin';

// https://vite.dev/config/
export default defineConfig({
  plugins: [heyApiPlugin({
    config: {
      client: '@hey-api/client-fetch',
      input: '../ProjectCowork.Api/project-cowork.json', // sign up at app.heyapi.dev
      output: 'src/client',
      plugins: [
        '@tanstack/react-query'
      ]
    },}),
    react()],
})
