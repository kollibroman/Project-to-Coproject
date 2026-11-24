import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { heyApiPlugin } from '@hey-api/vite-plugin';

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), 'VITE_')

  return {
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
    server: {
      proxy: {
        '/api': {
          target: env.VITE_API_URL || 'http://localhost:5298',
          changeOrigin: true,
        },
      },
    },
  }
})
