import type { ActionType, PermissionDto, ResourceType } from '../../client';

// Wartości muszą odpowiadać enumom ResourceType i ActionType z backendu (serializowane jako liczby).
export const ResourceTypeValue = {
    Projects: 0,
    Postings: 1,
    Repositories: 2,
    Applications: 3,
    Management: 4,
    Permissions: 5,
} as const satisfies Record<string, ResourceType>;

export const ActionTypeValue = {
    View: 0,
    Create: 1,
    Update: 2,
    Delete: 3,
} as const satisfies Record<string, ActionType>;

type Option<T> = { value: T; key: string; label: string; color: string };

export const resourceOptions: Option<ResourceType>[] = [
    { value: ResourceTypeValue.Projects, key: 'Projects', label: 'Projekty', color: 'blue' },
    { value: ResourceTypeValue.Postings, key: 'Postings', label: 'Ogłoszenia', color: 'grape' },
    { value: ResourceTypeValue.Repositories, key: 'Repositories', label: 'Repozytoria', color: 'teal' },
    { value: ResourceTypeValue.Applications, key: 'Applications', label: 'Zgłoszenia', color: 'cyan' },
    { value: ResourceTypeValue.Management, key: 'Management', label: 'Zarządzanie', color: 'orange' },
    { value: ResourceTypeValue.Permissions, key: 'Permissions', label: 'Uprawnienia', color: 'red' },
];

export const actionOptions: Option<ActionType>[] = [
    { value: ActionTypeValue.View, key: 'View', label: 'Odczyt', color: 'blue' },
    { value: ActionTypeValue.Create, key: 'Create', label: 'Tworzenie', color: 'green' },
    { value: ActionTypeValue.Update, key: 'Update', label: 'Edycja', color: 'yellow' },
    { value: ActionTypeValue.Delete, key: 'Delete', label: 'Usuwanie', color: 'red' },
];

const unknownOption = (value: number): Option<number> => ({
    value,
    key: String(value),
    label: `Nieznane (${value})`,
    color: 'gray',
});

export const getResource = (value: ResourceType) =>
    resourceOptions.find((x) => x.value === value) ?? unknownOption(value);

export const getAction = (value: ActionType) =>
    actionOptions.find((x) => x.value === value) ?? unknownOption(value);

/** Nazwa polityki autoryzacji po stronie API, np. „Permission.Projects.View”. */
export const getPolicyName = (permission: Pick<PermissionDto, 'resource' | 'action'>) =>
    `Permission.${getResource(permission.resource).key}.${getAction(permission.action).key}`;

/** API opisuje int32 jako `number | string`, a ścieżki oczekują liczby. */
export const getPermissionId = (permission: PermissionDto) => Number(permission.id);

/** Dane dla Mantine Select, który operuje na stringach. */
export const toSelectData = (options: { value: number; label: string }[]) =>
    options.map((x) => ({ value: String(x.value), label: x.label }));

const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const isGuid = (value: string) => GUID_REGEX.test(value.trim());
