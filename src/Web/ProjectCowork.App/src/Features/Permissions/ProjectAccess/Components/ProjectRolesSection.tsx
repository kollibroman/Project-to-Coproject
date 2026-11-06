import { ActionIcon, Badge, Button, Group, Paper, SimpleGrid, Stack, Text, Title, Tooltip } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { PencilSimpleIcon, PlusIcon, TrashIcon } from '@phosphor-icons/react';
import { useState } from 'react';
import type { PermissionDto } from '../../../../client';
import { ConfirmDeleteDialog } from '../../../Management/Dialogs/ConfirmDeleteDialog.tsx';
import { actionOptions, getPermissionId, resourceOptions } from '../../permissionEnums.ts';
import { ProjectRoleDialog } from '../Dialogs/ProjectRoleDialog.tsx';
import type { ProjectRole, ProjectRoleInput } from '../projectAccessTypes.ts';

interface Props {
    roles: ProjectRole[];
    catalog: PermissionDto[];
    onCreate: (input: ProjectRoleInput) => Promise<void>;
    onUpdate: (roleId: string, input: ProjectRoleInput) => Promise<void>;
    onDelete: (roleId: string) => Promise<void>;
}

const deleteBlockReason = (role: ProjectRole) => {
    if (role.isDefault) {
        return 'Nie można usunąć roli domyślnej';
    }
    if (role.memberCount > 0) {
        return 'Najpierw zmień rolę członkom';
    }
    return null;
};

export const ProjectRolesSection = ({ roles, catalog, onCreate, onUpdate, onDelete }: Props) => {
    const [dialogOpened, { open: openDialog, close: closeDialog }] = useDisclosure(false);
    const [selected, setSelected] = useState<ProjectRole | undefined>(undefined);
    const [deleteTarget, setDeleteTarget] = useState<ProjectRole | undefined>(undefined);
    const [deleteError, setDeleteError] = useState<Error | undefined>(undefined);

    // Liczba uprawnień roli w każdym zasobie, np. „Projekty 2/4”.
    const summarize = (role: ProjectRole) => {
        const granted = new Set(role.permissionIds);
        return resourceOptions
            .map((resource) => ({
                resource,
                count: catalog.filter((p) => p.resource === resource.value && granted.has(getPermissionId(p))).length,
            }))
            .filter((x) => x.count > 0);
    };

    const handleCreate = () => {
        setSelected(undefined);
        openDialog();
    };

    const handleEdit = (role: ProjectRole) => {
        setSelected(role);
        openDialog();
    };

    const closeDelete = () => {
        setDeleteTarget(undefined);
        setDeleteError(undefined);
    };

    const handleConfirmDelete = async () => {
        if (!deleteTarget) {
            return;
        }
        try {
            await onDelete(deleteTarget.id);
            closeDelete();
        } catch (e) {
            setDeleteError(e instanceof Error ? e : new Error('Nie udało się usunąć roli.'));
        }
    };

    return (
        <>
            <Stack gap="md">
                <Group justify="space-between">
                    <Stack gap={0}>
                        <Title order={3}>Role</Title>
                        <Text size="sm" c="dimmed">
                            Każda rola ma własny zestaw uprawnień w tym projekcie.
                        </Text>
                    </Stack>
                    <Button leftSection={<PlusIcon />} onClick={handleCreate}>
                        Nowa rola
                    </Button>
                </Group>

                <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }}>
                    {roles.map((role) => {
                        const blockReason = deleteBlockReason(role);
                        const summary = summarize(role);
                        return (
                            <Paper key={role.id} withBorder shadow="md" radius="sm" p="lg">
                                <Stack gap="sm">
                                    <Group justify="space-between" align="flex-start" wrap="nowrap">
                                        <Stack gap={4}>
                                            <Group gap="xs">
                                                <Title order={4}>{role.name}</Title>
                                                {role.isDefault && (
                                                    <Badge variant="light" color="green">
                                                        Domyślna
                                                    </Badge>
                                                )}
                                            </Group>
                                            <Text size="sm" c="dimmed">
                                                {role.permissionIds.length} z {catalog.length} uprawnień ·{' '}
                                                {role.memberCount} {role.memberCount === 1 ? 'członek' : 'członków'}
                                            </Text>
                                        </Stack>
                                        <Group gap="xs" wrap="nowrap">
                                            <Tooltip label="Edytuj rolę i uprawnienia">
                                                <ActionIcon
                                                    variant="light"
                                                    size="lg"
                                                    radius="sm"
                                                    onClick={() => handleEdit(role)}
                                                >
                                                    <PencilSimpleIcon style={{ width: '70%', height: '70%' }} />
                                                </ActionIcon>
                                            </Tooltip>
                                            <Tooltip label={blockReason ?? 'Usuń rolę'}>
                                                {/* span: wyłączony przycisk nie odbiera hovera, a tooltip ma pokazać powód */}
                                                <span>
                                                    <ActionIcon
                                                        variant="light"
                                                        color="red"
                                                        size="lg"
                                                        radius="sm"
                                                        disabled={Boolean(blockReason)}
                                                        onClick={() => setDeleteTarget(role)}
                                                    >
                                                        <TrashIcon style={{ width: '70%', height: '70%' }} />
                                                    </ActionIcon>
                                                </span>
                                            </Tooltip>
                                        </Group>
                                    </Group>

                                    {summary.length === 0 ? (
                                        <Text size="sm" c="dimmed">
                                            Brak uprawnień.
                                        </Text>
                                    ) : (
                                        <Group gap={6}>
                                            {summary.map(({ resource, count }) => (
                                                <Badge key={resource.value} color={resource.color} variant="light">
                                                    {resource.label} {count}/{actionOptions.length}
                                                </Badge>
                                            ))}
                                        </Group>
                                    )}
                                </Stack>
                            </Paper>
                        );
                    })}
                </SimpleGrid>
            </Stack>

            <ProjectRoleDialog
                role={selected}
                catalog={catalog}
                opened={dialogOpened}
                onClose={closeDialog}
                onSubmit={(input) => (selected ? onUpdate(selected.id, input) : onCreate(input))}
            />

            <ConfirmDeleteDialog
                opened={Boolean(deleteTarget)}
                title="Usuń rolę"
                message={`Czy na pewno usunąć rolę „${deleteTarget?.name ?? ''}”?`}
                error={deleteError}
                onConfirm={handleConfirmDelete}
                onClose={closeDelete}
            />
        </>
    );
};
