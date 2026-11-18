import type { Locator, Page } from '@playwright/test';
import { addMember, createProject, createSprint, createTask, registerAccount, unique } from './support/api';
import { expect, test } from './support/fixtures';

const sprintSection = (page: Page, name: string) =>
  page.locator(`[data-testid="sprint-section"][data-sprint-name="${name}"]`);

const taskCard = (scope: Page | Locator, name: string) =>
  scope.locator(`[data-testid="task"][data-task-name="${name}"]`);

async function pick(page: Page, field: Locator, option: string) {
  await field.click();
  await page.getByRole('option', { name: option, exact: true }).click();
}

test.describe('sprinty i zadania', () => {
  test('nowe zadanie w sprincie, zmiana statusu na „Zrobione”', async ({ userPage: page, user }) => {
    const project = await createProject(user);
    const sprint = await createSprint(user, project.id);
    const taskName = unique('Zadanie');

    await page.goto(`/project/${project.id}?tab=sprints`);
    await page.getByRole('button', { name: 'Nowe zadanie' }).click();
    const dialog = page.getByRole('dialog', { name: 'Nowe zadanie' });
    await dialog.getByLabel('Nazwa').fill(taskName);
    await pick(page, dialog.getByRole('combobox', { name: 'Priorytet' }), 'Pilny');
    await pick(page, dialog.getByRole('combobox', { name: 'Sprint' }), sprint.name);
    await dialog.getByRole('button', { name: 'Utwórz' }).click();
    await expect(dialog).toBeHidden();

    const section = sprintSection(page, sprint.name);
    await expect(section.getByText('1 zadanie')).toBeVisible();
    await expect(section.getByText('Ukończone: 0/1')).toBeVisible();
    await expect(taskCard(sprintSection(page, 'Backlog'), taskName)).toHaveCount(0);

    await section.getByRole('link', { name: 'Otwórz sprint' }).click();
    await expect(page.getByRole('heading', { name: sprint.name, level: 1 })).toBeVisible();

    const task = taskCard(page, taskName);
    await expect(task.getByText('#1')).toBeVisible();

    await pick(page, task.getByRole('combobox', { name: 'Status' }), 'Zrobione');

    await expect(task.getByText(/^Ukończono: \d{4}-\d{2}-\d{2}$/)).toBeVisible();

    await page.reload();
    await expect(taskCard(page, taskName).getByRole('combobox', { name: 'Status' })).toHaveValue('Zrobione');

    await page.goto(`/project/${project.id}?tab=sprints`);
    await expect(sprintSection(page, sprint.name).getByText('Ukończone: 1/1')).toBeVisible();
  });

  test('usunięcie sprintu przenosi jego zadania do backlogu', async ({ userPage: page, user }) => {
    const project = await createProject(user);
    const sprint = await createSprint(user, project.id);
    const taskName = unique('Zadanie');

    await page.goto(`/project/${project.id}?tab=sprints`);
    await sprintSection(page, sprint.name).getByRole('button', { name: 'Dodaj zadanie' }).click();
    const dialog = page.getByRole('dialog', { name: 'Nowe zadanie' });
    await expect(dialog.getByRole('combobox', { name: 'Sprint' })).toHaveValue(sprint.name);
    await dialog.getByLabel('Nazwa').fill(taskName);
    await dialog.getByRole('button', { name: 'Utwórz' }).click();
    await expect(sprintSection(page, sprint.name).getByText('1 zadanie')).toBeVisible();
    await expect(taskCard(sprintSection(page, 'Backlog'), taskName)).toHaveCount(0);

    await sprintSection(page, sprint.name).getByRole('button', { name: 'Usuń sprint' }).click();
    const confirm = page.getByRole('dialog', { name: 'Usuń sprint' });
    await expect(confirm.getByText('Jego zadania wrócą do backlogu.')).toBeVisible();
    await confirm.getByRole('button', { name: 'Usuń' }).click();

    await expect(sprintSection(page, sprint.name)).toHaveCount(0);
    await expect(taskCard(sprintSection(page, 'Backlog'), taskName)).toBeVisible();
  });

  test('usunięcie zadania po potwierdzeniu', async ({ userPage: page, user }) => {
    const project = await createProject(user);
    const task = await createTask(user, project.id);

    await page.goto(`/project/${project.id}?tab=sprints`);
    await taskCard(page, task.name).getByRole('button', { name: 'Usuń zadanie' }).click();
    await page.getByRole('dialog', { name: 'Usuń zadanie' }).getByRole('button', { name: 'Usuń' }).click();

    await expect(taskCard(page, task.name)).toHaveCount(0);
  });

  test('obserwator dostaje komunikat błędu przy próbie dodania zadania', async ({ userPage: page, user }) => {
    const owner = await registerAccount();
    const project = await createProject(owner);
    await addMember(owner, project.id, user, 'Obserwator');

    await page.goto(`/project/${project.id}?tab=sprints`);
    await page.getByRole('button', { name: 'Nowe zadanie' }).click();
    const dialog = page.getByRole('dialog', { name: 'Nowe zadanie' });
    await dialog.getByLabel('Nazwa').fill(unique('Zadanie'));
    await dialog.getByRole('button', { name: 'Utwórz' }).click();

    await expect(dialog.getByRole('alert')).toContainText('Błąd');
    await expect(dialog).toBeVisible();
  });
});
