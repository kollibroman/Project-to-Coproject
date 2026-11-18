import { PASSWORD, registerAccount, unique } from './support/api';
import { expect, signIn, test } from './support/fixtures';

test.describe('logowanie i sesja', () => {
  test('niezalogowany trafia na /auth', async ({ page }) => {
    await page.goto('/projects');

    await expect(page).toHaveURL(/\/auth$/);
    await expect(page.getByRole('tab', { name: 'Logowanie' })).toHaveAttribute('aria-selected', 'true');
  });

  test('formularz logowania pokazuje błędy walidacji', async ({ page }) => {
    await page.goto('/auth');
    const panel = page.getByRole('tabpanel', { name: 'Logowanie' });

    await panel.getByLabel('E-mail').fill('to-nie-email');
    await panel.getByLabel('Hasło').fill('123');
    await panel.getByRole('button', { name: 'Zaloguj się' }).click();

    await expect(panel.getByText('Nieprawidłowy adres e-mail')).toBeVisible();
    await expect(panel.getByText('Hasło musi mieć min. 6 znaków')).toBeVisible();
    await expect(page).toHaveURL(/\/auth$/);
  });

  test('rejestracja przez formularz loguje i otwiera pulpit', async ({ page }) => {
    const username = unique('ui');

    await page.goto('/auth');
    await page.getByRole('tab', { name: 'Rejestracja' }).click();
    const panel = page.getByRole('tabpanel', { name: 'Rejestracja' });

    await panel.getByLabel('Nazwa użytkownika').fill(username);
    await panel.getByLabel('E-mail').fill(`${username}@example.com`);
    await panel.getByRole('textbox', { name: 'Hasło', exact: true }).fill(PASSWORD);
    await panel.getByLabel('Powtórz hasło').fill(PASSWORD);
    await panel.getByRole('button', { name: 'Zarejestruj się' }).click();

    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { name: 'Pulpit' })).toBeVisible();
  });

  test('rejestracja wykrywa różne hasła', async ({ page }) => {
    await page.goto('/auth');
    await page.getByRole('tab', { name: 'Rejestracja' }).click();
    const panel = page.getByRole('tabpanel', { name: 'Rejestracja' });

    await panel.getByRole('textbox', { name: 'Hasło', exact: true }).fill(PASSWORD);
    await panel.getByLabel('Powtórz hasło').fill('Inne1234!');
    await panel.getByRole('button', { name: 'Zarejestruj się' }).click();

    await expect(panel.getByText('Hasła nie są takie same')).toBeVisible();
  });

  test('logowanie przez formularz', async ({ page, user }) => {
    await page.goto('/auth');
    const panel = page.getByRole('tabpanel', { name: 'Logowanie' });

    await panel.getByLabel('E-mail').fill(user.email);
    await panel.getByLabel('Hasło').fill(user.password);
    await panel.getByRole('button', { name: 'Zaloguj się' }).click();

    await expect(page.getByRole('heading', { name: 'Pulpit' })).toBeVisible();
  });

  test('sesja przeżywa przeładowanie strony', async ({ userPage: page }) => {
    await page.goto('/projects');
    await expect(page.getByRole('heading', { name: 'Projekty' })).toBeVisible();

    await page.reload();

    await expect(page).toHaveURL(/\/projects$/);
    await expect(page.getByRole('heading', { name: 'Projekty' })).toBeVisible();
  });

  test('wylogowanie kończy sesję także po przeładowaniu', async ({ userPage: page }) => {
    await page.goto('/');
    // Czekamy na odpowiedź serwera. Bez tego page.goto mógłby przerwać żądanie unieważniające refresh token.
    const loggedOut = page.waitForResponse((r) => r.url().endsWith('/api/auth/logout'));
    await page.getByRole('button', { name: 'Wyloguj' }).click();
    expect((await loggedOut).status()).toBe(204);
    await expect(page).toHaveURL(/\/auth$/);

    await page.goto('/projects');

    await expect(page).toHaveURL(/\/auth$/);
  });

  test('wylogowanie w jednej karcie wylogowuje drugą', async ({ userPage: page }) => {
    const secondTab = await page.context().newPage();
    await page.goto('/projects');
    await secondTab.goto('/projects');
    await expect(secondTab.getByRole('heading', { name: 'Projekty' })).toBeVisible();

    await page.getByRole('button', { name: 'Wyloguj' }).click();

    await expect(secondTab).toHaveURL(/\/auth$/);
  });

  test('konto firmowe widzi w menu „Odkryj” zamiast „Projekty”', async ({ page }) => {
    const business = await registerAccount('business');
    await signIn(page, business);

    await page.goto('/');
    const navbar = page.locator('nav');

    await expect(navbar.getByText('Odkryj', { exact: true })).toBeVisible();
    await expect(navbar.getByText('Projekty', { exact: true })).toHaveCount(0);
  });
});
