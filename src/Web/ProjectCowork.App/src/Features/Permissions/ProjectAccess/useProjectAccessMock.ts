import { useMemo, useState } from 'react';
import type { PermissionDto } from '../../../client';
import { ActionTypeValue, ResourceTypeValue, actionOptions, resourceOptions } from '../permissionEnums.ts';
import type { ProjectMember, ProjectRole, ProjectRoleInput } from './projectAccessTypes.ts';

// DANE PRZYKŁADOWE: widok nie jest jeszcze podłączony do API.
// Hook trzyma stan lokalnie i odtwarza reguły z backendu (sekcja 1 planu), żeby UI zachowywał się jak docelowo.
// Po podłączeniu API zastąp go hookami React Query o tym samym kształcie.

const permissionId = (resource: number, action: number) => resource * actionOptions.length + action + 1;

export const mockPermissionCatalog: PermissionDto[] = resourceOptions.flatMap((resource) =>
    actionOptions.map((action) => ({
        id: permissionId(resource.value, action.value),
        resource: resource.value,
        action: action.value,
    })),
);

export const allPermissionIds = mockPermissionCatalog.map((x) => Number(x.id));

export const viewPermissionIds = mockPermissionCatalog
    .filter((x) => x.action === ActionTypeValue.View)
    .map((x) => Number(x.id));

type StoredRole = Omit<ProjectRole, 'memberCount'>;
type StoredMember = Omit<ProjectMember, 'roleName' | 'isOwner' | 'roleId'> & { roleId: string };

const initialRoles: StoredRole[] = [
    {
        id: 'role-admin',
        name: 'Administrator',
        isDefault: false,
        permissionIds: allPermissionIds,
    },
    {
        id: 'role-member',
        name: 'Członek',
        isDefault: true,
        permissionIds: [
            ...viewPermissionIds,
            permissionId(ResourceTypeValue.Management, ActionTypeValue.Create),
            permissionId(ResourceTypeValue.Management, ActionTypeValue.Update),
            permissionId(ResourceTypeValue.Management, ActionTypeValue.Delete),
            permissionId(ResourceTypeValue.Repositories, ActionTypeValue.Create),
        ],
    },
    {
        id: 'role-viewer',
        name: 'Obserwator',
        isDefault: false,
        permissionIds: viewPermissionIds,
    },
];

const initialMembers: StoredMember[] = [
    { userId: 'user-1', userName: 'anna.kowalska', email: 'anna.kowalska@example.com', roleId: 'role-admin' },
    { userId: 'user-2', userName: 'jan.nowak', email: 'jan.nowak@example.com', roleId: 'role-member' },
    { userId: 'user-3', userName: 'ola.wisniewska', email: 'ola.wisniewska@example.com', roleId: 'role-member' },
    { userId: 'user-4', userName: 'piotr.zielinski', email: 'piotr.zielinski@example.com', roleId: 'role-viewer' },
];

const sameName = (a: string, b: string) => a.trim().toLocaleLowerCase() === b.trim().toLocaleLowerCase();

const newId = () =>
    typeof crypto !== 'undefined' && 'randomUUID' in crypto
        ? crypto.randomUUID()
        : `id-${Date.now()}-${Math.random().toString(16).slice(2)}`;

export const useProjectAccessMock = (ownerId: string | undefined) => {
    const [storedRoles, setStoredRoles] = useState<StoredRole[]>(initialRoles);
    const [storedMembers, setStoredMembers] = useState<StoredMember[]>(initialMembers);

    const roles: ProjectRole[] = useMemo(
        () =>
            storedRoles.map((role) => ({
                ...role,
                memberCount: storedMembers.filter((m) => m.roleId === role.id).length,
            })),
        [storedRoles, storedMembers],
    );

    const members: ProjectMember[] = useMemo(
        () => [
            {
                userId: ownerId ?? 'owner',
                userName: 'Właściciel projektu',
                email: '—',
                roleId: null,
                roleName: 'Właściciel',
                isOwner: true,
            },
            ...storedMembers.map((member) => ({
                ...member,
                roleName: storedRoles.find((r) => r.id === member.roleId)?.name ?? '—',
                isOwner: false,
            })),
        ],
        [ownerId, storedMembers, storedRoles],
    );

    const validateRole = (input: ProjectRoleInput, roleId?: string) => {
        if (input.name.trim().length === 0) {
            throw new Error('Nazwa roli jest wymagana.');
        }
        if (storedRoles.some((r) => r.id !== roleId && sameName(r.name, input.name))) {
            throw new Error(`Rola „${input.name.trim()}” już istnieje w tym projekcie.`);
        }
    };

    // Tylko jedna rola może być domyślna, więc ustawienie nowej zdejmuje flagę z poprzedniej.
    const withDefault = (list: StoredRole[], defaultId: string, isDefault: boolean) =>
        isDefault ? list.map((r) => ({ ...r, isDefault: r.id === defaultId })) : list;

    const createRole = (input: ProjectRoleInput) => {
        validateRole(input);
        const role: StoredRole = {
            id: newId(),
            name: input.name.trim(),
            isDefault: input.isDefault,
            permissionIds: [...input.permissionIds],
        };
        setStoredRoles((prev) => withDefault([...prev, role], role.id, input.isDefault));
    };

    const updateRole = (roleId: string, input: ProjectRoleInput) => {
        validateRole(input, roleId);
        const current = storedRoles.find((r) => r.id === roleId);
        if (!current) {
            throw new Error('Rola nie istnieje.');
        }
        if (current.isDefault && !input.isDefault) {
            throw new Error('Projekt musi mieć rolę domyślną. Ustaw inną rolę jako domyślną.');
        }
        setStoredRoles((prev) =>
            withDefault(
                prev.map((r) =>
                    r.id === roleId
                        ? { ...r, name: input.name.trim(), isDefault: input.isDefault, permissionIds: [...input.permissionIds] }
                        : r,
                ),
                roleId,
                input.isDefault,
            ),
        );
    };

    const deleteRole = (roleId: string) => {
        const role = storedRoles.find((r) => r.id === roleId);
        if (!role) {
            return;
        }
        if (role.isDefault) {
            throw new Error('Nie można usunąć roli domyślnej.');
        }
        if (storedMembers.some((m) => m.roleId === roleId)) {
            throw new Error('Nie można usunąć roli, która ma przypisanych członków.');
        }
        setStoredRoles((prev) => prev.filter((r) => r.id !== roleId));
    };

    const inviteMember = (email: string, roleId: string | null) => {
        const normalized = email.trim().toLocaleLowerCase();
        if (storedMembers.some((m) => m.email.toLocaleLowerCase() === normalized)) {
            throw new Error('Ten użytkownik jest już członkiem projektu.');
        }
        const targetRoleId = roleId ?? storedRoles.find((r) => r.isDefault)?.id;
        if (!targetRoleId) {
            throw new Error('Brak roli domyślnej w projekcie.');
        }
        setStoredMembers((prev) => [
            ...prev,
            { userId: newId(), userName: normalized.split('@')[0], email: normalized, roleId: targetRoleId },
        ]);
    };

    const changeMemberRole = (userId: string, roleId: string) => {
        setStoredMembers((prev) => prev.map((m) => (m.userId === userId ? { ...m, roleId } : m)));
    };

    const removeMember = (userId: string) => {
        setStoredMembers((prev) => prev.filter((m) => m.userId !== userId));
    };

    return {
        catalog: mockPermissionCatalog,
        roles,
        members,
        createRole,
        updateRole,
        deleteRole,
        inviteMember,
        changeMemberRole,
        removeMember,
    };
};

export type ProjectAccessState = ReturnType<typeof useProjectAccessMock>;
