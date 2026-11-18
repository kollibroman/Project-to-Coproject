import { defineConfig, devices } from '@playwright/test';
import { API_URL, APP_URL } from './e2e/support/env';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: process.env.CI ? [['github'], ['html', { open: 'never' }]] : [['list'], ['html', { open: 'never' }]],
  timeout: 30_000,
  expect: { timeout: 10_000 },

  use: {
    baseURL: APP_URL,
    locale: 'pl-PL',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],

  webServer: process.env.E2E_EXTERNAL
    ? undefined
    : {
        command:
          'docker compose -p projectcowork-e2e -f compose.yaml -f compose.e2e.yaml up --build --abort-on-container-failure',
        cwd: '../..',
        env: { API_PORT: process.env.API_PORT ?? '18080', APP_PORT: process.env.APP_PORT ?? '13000' },
        url: `${API_URL}/api/permissions`,
        reuseExistingServer: !process.env.CI,
        timeout: 15 * 60_000,
        stdout: 'ignore',
        stderr: 'pipe',
        gracefulShutdown: { signal: 'SIGTERM', timeout: 30_000 },
      },
});
