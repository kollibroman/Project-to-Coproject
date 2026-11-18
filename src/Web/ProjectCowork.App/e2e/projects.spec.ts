import { addMember, createProject, registerAccount, unique } from './support/api';
import { expect, openAs, test } from './support/fixtures';

test.describe('projekty i zaproszenia', () => {
  test('utworzenie projektu z listy projektów', async ({ userPage: page }) => {
    const name = unique('Projekt');

    await page.goto('/projects');
    await page.getByRole('button', { name: 'Nowy projekt' }).click();
    const dialog = page.getByRole('dialog', { name: 'Nowy projekt' });
    await dialog.getByLabel('Nazwa').fill(name);
    await dialog.getByLabel('Opis').fill('Opis z testu');
    await dialog.getByRole('button', { name: 'Utwórz' }).click();

    await expect(dialog).toBeHidden();
    await expect(page.getByRole('heading', { name })).toBeVisible();
    await expect(page.getByText('Rola: Owner')).toBeVisible();

    await page.getByRole('link', { name: `Otwórz projekt ${name}` }).click();

    await expect(page.getByRole('heading', { name, level: 1 })).toBeVisible();
    await expect(page.getByText('Opis z testu')).toBeVisible();
  });

  test('pusta nazwa projektu jest odrzucana', async ({ userPage: page }) => {
    await page.goto('/projects');
    await page.getByRole('button', { name: 'Nowy projekt' }).click();
    const dialog = page.getByRole('dialog', { name: 'Nowy projekt' });
    await dialog.getByRole('button', { name: 'Utwórz' }).click();

    await expect(dialog.getByText('Nazwa jest wymagana')).toBeVisible();
  });

  test('cudzy projekt otwarty z adresu przekierowuje na NotFound', async ({ userPage: page }) => {
    const owner = await registerAccount();
    const project = await createProject(owner);

    await page.goto(`/project/${project.id}`);

    await expect(page).toHaveURL(/\/NotFound$/, { timeout: 20_000 });
  });

  test('zaproszenie przez UI, akceptacja dzwonkiem i nowy członek na liście', async ({ browser, user: owner }) => {
    const invitee = await registerAccount();
    const project = await createProject(owner);

    const ownerPage = await openAs(browser, owner);
    await ownerPage.goto(`/project/${project.id}?tab=access`);
    await ownerPage.getByRole('button', { name: 'Zaproś' }).click();
    const dialog = ownerPage.getByRole('dialog', { name: 'Zaproś do projektu' });
    await dialog.getByLabel('E-mail').fill(invitee.email);
    await dialog.getByRole('button', { name: 'Zaproś' }).click();
    await expect(dialog).toBeHidden();

    const inviteePage = await openAs(browser, invitee);
    await inviteePage.goto('/');
    await inviteePage.getByRole('button', { name: 'Masz zaproszenie do projektu' }).click();

    await expect(inviteePage).toHaveURL(/\/account#invitations$/);
    await expect(inviteePage.getByText(project.name)).toBeVisible();
    await inviteePage.getByRole('button', { name: 'Dołącz' }).click();

    await expect(inviteePage).toHaveURL(new RegExp(`/project/${project.id}$`));
    await expect(inviteePage.getByRole('heading', { name: project.name, level: 1 })).toBeVisible();

    await ownerPage.reload();
    await expect(ownerPage.getByText(invitee.username, { exact: true })).toBeVisible();
    // Mantine Select ma rolę combobox, a wybrana opcja jest jego wartością.
    await expect(ownerPage.getByRole('combobox', { name: `Rola użytkownika ${invitee.username}` }))
      .toHaveValue('Członek');
  });

  test('odrzucone zaproszenie znika i nie daje dostępu', async ({ browser, user: owner }) => {
    const invitee = await registerAccount();
    const project = await createProject(owner);

    const ownerPage = await openAs(browser, owner);
    await ownerPage.goto(`/project/${project.id}?tab=access`);
    await ownerPage.getByRole('button', { name: 'Zaproś' }).click();
    const dialog = ownerPage.getByRole('dialog', { name: 'Zaproś do projektu' });
    await dialog.getByLabel('E-mail').fill(invitee.email);
    await dialog.getByRole('button', { name: 'Zaproś' }).click();

    const inviteePage = await openAs(browser, invitee);
    await inviteePage.goto('/account');
    await inviteePage.getByRole('button', { name: 'Odrzuć' }).click();

    await expect(inviteePage.getByText('Nie masz oczekujących zaproszeń.')).toBeVisible();
    await expect(inviteePage.getByRole('button', { name: /Masz zaproszenie/ })).toHaveCount(0);

    await inviteePage.goto('/projects');
    await expect(inviteePage.getByText('Brak projektów.')).toBeVisible();
  });

  test('członek widzi projekt na swojej liście z nazwą roli', async ({ userPage: page, user: member }) => {
    const owner = await registerAccount();
    const project = await createProject(owner);
    await addMember(owner, project.id, member, 'Obserwator');

    await page.goto('/projects');

    await expect(page.getByRole('heading', { name: project.name })).toBeVisible();
    await expect(page.getByText('Rola: Obserwator')).toBeVisible();
  });
});
