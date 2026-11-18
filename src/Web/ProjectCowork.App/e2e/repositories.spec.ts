import { execFileSync } from 'node:child_process';
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { createProject, createRepository, unique } from './support/api';
import { expect, test } from './support/fixtures';

function pushFiles(cloneUrl: string, files: Record<string, string>, message: string) {
  const dir = mkdtempSync(join(tmpdir(), 'projectcowork-e2e-'));
  const git = (...args: string[]) =>
    execFileSync('git', ['-c', 'user.name=E2E Tester', '-c', 'user.email=e2e@example.com', ...args], {
      cwd: dir,
      env: { ...process.env, GIT_CONFIG_GLOBAL: '/dev/null', GIT_CONFIG_NOSYSTEM: '1', GIT_TERMINAL_PROMPT: '0' },
      stdio: 'pipe',
    });

  try {
    git('init', '-b', 'main');
    for (const [path, content] of Object.entries(files)) {
      mkdirSync(dirname(join(dir, path)), { recursive: true });
      writeFileSync(join(dir, path), content);
    }
    git('add', '--all');
    git('commit', '-m', message);
    git('push', cloneUrl, 'main');
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
}

test.describe('repozytoria', () => {
  test('dodanie repozytorium przez UI pokazuje instrukcję dla pustego repozytorium', async ({ userPage: page, user }) => {
    const project = await createProject(user);
    const name = unique('repo');

    await page.goto(`/project/${project.id}`);
    await page.getByRole('button', { name: 'Dodaj repozytorium' }).click();
    const dialog = page.getByRole('dialog', { name: 'Nowe repozytorium' });
    await dialog.getByLabel('Nazwa').fill(name);
    await dialog.getByRole('button', { name: 'Utwórz' }).click();
    await expect(dialog).toBeHidden();

    await page.getByRole('link', { name: `Otwórz repozytorium ${name}` }).click();

    await expect(page.getByRole('heading', { name, level: 1 })).toBeVisible();
    await expect(page.getByText('To repozytorium jest puste')).toBeVisible();
    await expect(page.locator('input[readonly]').last())
      .toHaveValue(new RegExp(`git remote add origin http://.+/api/git/${project.id}/${name}\\.git`));
  });

  test('po git push widać pliki, foldery, zawartość i commity', async ({ userPage: page, user }) => {
    const project = await createProject(user);
    const repository = await createRepository(user, project.id);
    pushFiles(
      repository.cloneUrl,
      { 'README.md': '# Projekt E2E\n', 'src/main.ts': 'export const answer = 42;\n' },
      'Pierwszy commit z E2E',
    );

    await page.goto(`/project/${project.id}`);
    await page.getByRole('link', { name: `Otwórz repozytorium ${repository.name}` }).click();

    const files = page.getByRole('table');
    await expect(files.getByRole('row').nth(0)).toContainText('src');
    await expect(files.getByRole('row').nth(1)).toContainText('README.md');

    await files.getByText('src', { exact: true }).click();
    await expect(page).toHaveURL(/path=src/);
    await files.getByText('main.ts').click();

    await expect(page).toHaveURL(/file=src%2Fmain\.ts|file=src\/main\.ts/);
    await expect(page.getByText('export const answer = 42;')).toBeVisible();

    await page.getByRole('tab', { name: 'Commity' }).click();
    await expect(page.getByText('Pierwszy commit z E2E')).toBeVisible();
    await expect(page.getByText(/^E2E Tester · /)).toBeVisible();
  });
});
