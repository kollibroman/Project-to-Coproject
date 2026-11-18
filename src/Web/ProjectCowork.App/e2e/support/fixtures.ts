import { test as base, expect, type Browser, type Page } from '@playwright/test';
import { registerAccount, type Account } from './api';
import { API_URL } from './env';

export async function signIn(page: Page, account: Account) {
  const response = await page.request.post(`${API_URL}/api/auth/login`, {
    data: { email: account.email, password: account.password },
  });
  expect(response.status(), await response.text()).toBe(200);
}

export async function openAs(browser: Browser, account: Account): Promise<Page> {
  const context = await browser.newContext();
  const page = await context.newPage();
  await signIn(page, account);
  return page;
}

type Fixtures = {
 user: Account;
 userPage: Page;
};

export const test = base.extend<Fixtures>({
  user: async ({}, use) => {
    await use(await registerAccount('user'));
  },
  userPage: async ({ page, user }, use) => {
    await signIn(page, user);
    await use(page);
  },
});

export { expect };
