// Adresy stosu E2E. Muszą się zgadzać z playwright.config.ts i compose.e2e.yaml.
const API_PORT = process.env.API_PORT ?? '18080';
const APP_PORT = process.env.APP_PORT ?? '13000';

export const APP_URL = process.env.APP_URL ?? `http://localhost:${APP_PORT}`;
export const API_URL = process.env.API_URL ?? `http://localhost:${API_PORT}`;
