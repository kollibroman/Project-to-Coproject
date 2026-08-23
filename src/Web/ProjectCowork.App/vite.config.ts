import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { heyApiPlugin } from '@hey-api/vite-plugin';

// https://vite.dev/config/
export default defineConfig({
  plugins: [heyApiPlugin({
    config: {
      input: 'hey-api/backend', // sign up at app.heyapi.dev
      output: 'src/client',
    },}),
    react()],
})
