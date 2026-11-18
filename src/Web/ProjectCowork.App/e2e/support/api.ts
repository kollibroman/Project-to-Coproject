import { expect, request, type APIRequestContext } from '@playwright/test';
import { randomUUID } from 'node:crypto';
import { API_URL } from './env';

export const PASSWORD = 'Test1234!';

export type Account = {
  id: string;
  email: string;
  username: string;
  password: string;
  token: string;
};

export const unique = (prefix: string) => `${prefix}-${randomUUID().slice(0, 8)}`;

const authHeaders = (token: string) => ({ Authorization: `Bearer ${token}` });

let shared: APIRequestContext | undefined;

async function api(): Promise<APIRequestContext> {
  shared ??= await request.newContext({ baseURL: API_URL });
  return shared;
}

function userIdFromToken(token: string): string {
  const payload = JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString('utf8')) as Record<string, string>;
  return payload.nameid ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
}

export async function registerAccount(kind: 'user' | 'business' = 'user'): Promise<Account> {
  const username = unique('e2e');
  const email = `${username}@example.com`;

  const response = await (await api()).post(`/api/auth/register/${kind}`, {
    data: { email, username, password: PASSWORD },
  });
  expect(response.status(), await response.text()).toBe(200);

  const { accessToken } = (await response.json()) as { accessToken: string };
  return { id: userIdFromToken(accessToken), email, username, password: PASSWORD, token: accessToken };
}

export type Project = { id: string; name: string };

export async function createProject(owner: Account, name = unique('Projekt')): Promise<Project> {
  const ctx = await api();
  const created = await ctx.post('/api/projects', {
    headers: authHeaders(owner.token),
    data: { name, description: 'Projekt z testów E2E' },
  });
  expect(created.status(), await created.text()).toBe(201);

  const list = (await (await ctx.get('/api/projects', { headers: authHeaders(owner.token) })).json()) as Project[];
  return list.find((p) => p.name === name)!;
}

export async function createSprint(owner: Account, projectId: string, name = unique('Sprint')) {
  const ctx = await api();
  const created = await ctx.post('/api/sprints', {
    headers: authHeaders(owner.token),
    data: { projectId, name, startDate: '2026-11-01', endDate: '2026-11-14' },
  });
  expect(created.status(), await created.text()).toBe(201);
  return { name };
}

export async function createTask(owner: Account, projectId: string, name = unique('Zadanie')) {
  const created = await (await api()).post('/api/tasks', {
    headers: authHeaders(owner.token),
    data: { projectId, name, description: '', priority: 2, status: 0 },
  });
  expect(created.status(), await created.text()).toBe(201);
  return { name };
}

type Role = { id: string; name: string };

export async function addMember(owner: Account, projectId: string, member: Account, roleName: string) {
  const ctx = await api();
  const roles = (await (
    await ctx.get(`/api/projects/${projectId}/roles`, { headers: authHeaders(owner.token) })
  ).json()) as Role[];
  const roleId = roles.find((r) => r.name === roleName)!.id;

  await ctx.post(`/api/projects/${projectId}/invite`, {
    headers: authHeaders(owner.token),
    data: { projectId, email: member.email, roleId },
  });

  const invitations = (await (
    await ctx.get('/api/projects/invitations', { headers: authHeaders(member.token) })
  ).json()) as { id: string; projectId: string }[];
  const invitation = invitations.find((i) => i.projectId === projectId);
  expect(invitation, 'zaproszenie powinno istnieć').toBeDefined();

  const accepted = await ctx.post(`/api/projects/invitations/${invitation!.id}/accept`, {
    headers: authHeaders(member.token),
  });
  expect(accepted.status(), await accepted.text()).toBe(204);
}

export async function createRepository(owner: Account, projectId: string, name = unique('repo')) {
  const ctx = await api();
  const created = await ctx.post('/api/git/repository/create', {
    headers: authHeaders(owner.token),
    data: { projectId, name },
  });
  expect(created.status(), await created.text()).toBe(200);
  const cloneUrl = (await created.json()) as string;
  return { name, cloneUrl };
}
