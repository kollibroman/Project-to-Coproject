import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
  input: '../ProjectCowork.Api/project-cowork.json',
  output: 'src/client',
  plugins: ['@hey-api/client-fetch', '@tanstack/react-query'],
});
