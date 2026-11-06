import { Checkbox, Table, Text, Tooltip } from '@mantine/core';
import { useMemo } from 'react';
import type { PermissionDto } from '../../../client';
import { actionOptions, getPermissionId, getPolicyName, resourceOptions } from '../permissionEnums.ts';

interface Props {
    /** Wszystkie zdefiniowane uprawnienia (katalog). */
    permissions: PermissionDto[];
    /** Id uprawnień nadanych roli lub użytkownikowi. */
    granted: Set<number>;
    /** Brak handlera oznacza tryb tylko do odczytu. */
    onToggle?: (permission: PermissionDto, granted: boolean) => void;
    pendingId?: number;
    disabled?: boolean;
}

/** Macierz RBAC: wiersze to zasoby, kolumny to akcje. */
export const PermissionMatrix = ({ permissions, granted, onToggle, pendingId, disabled }: Props) => {
    const byKey = useMemo(() => {
        const map = new Map<string, PermissionDto>();
        for (const permission of permissions) {
            map.set(`${permission.resource}:${permission.action}`, permission);
        }
        return map;
    }, [permissions]);

    return (
        <Table.ScrollContainer minWidth={600}>
            <Table striped highlightOnHover withTableBorder>
                <Table.Thead>
                    <Table.Tr>
                        <Table.Th>Zasób</Table.Th>
                        {actionOptions.map((action) => (
                            <Table.Th key={action.value} ta="center">
                                {action.label}
                            </Table.Th>
                        ))}
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {resourceOptions.map((resource) => (
                        <Table.Tr key={resource.value}>
                            <Table.Td>
                                <Text fw={500}>{resource.label}</Text>
                            </Table.Td>
                            {actionOptions.map((action) => {
                                const permission = byKey.get(`${resource.value}:${action.value}`);

                                if (!permission) {
                                    return (
                                        <Table.Td key={action.value} ta="center">
                                            <Tooltip label="Uprawnienie nie jest zdefiniowane w katalogu">
                                                <Text c="dimmed" span>
                                                    —
                                                </Text>
                                            </Tooltip>
                                        </Table.Td>
                                    );
                                }

                                const id = getPermissionId(permission);
                                const checked = granted.has(id);

                                return (
                                    <Table.Td key={action.value} ta="center">
                                        <Tooltip label={getPolicyName(permission)}>
                                            <Checkbox
                                                display="inline-block"
                                                aria-label={`${resource.label}: ${action.label}`}
                                                checked={checked}
                                                readOnly={!onToggle}
                                                disabled={disabled || pendingId === id}
                                                onChange={(event) => onToggle?.(permission, event.currentTarget.checked)}
                                            />
                                        </Tooltip>
                                    </Table.Td>
                                );
                            })}
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
        </Table.ScrollContainer>
    );
};
