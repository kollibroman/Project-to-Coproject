import { ActionIcon, Badge, Button, Group, Select, Stack, Table, Text, Title, Tooltip } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { CrownIcon, TrashIcon, UserPlusIcon } from '@phosphor-icons/react';
import { useState } from 'react';
import { ConfirmDeleteDialog } from '../../../Management/Dialogs/ConfirmDeleteDialog.tsx';
import { InviteMemberDialog } from '../Dialogs/InviteMemberDialog.tsx';
import type { ProjectMember, ProjectRole } from '../projectAccessTypes.ts';

interface Props {
    members: ProjectMember[];
    roles: ProjectRole[];
    onInvite: (email: string, roleId: string | null) => void;
    onChangeRole: (userId: string, roleId: string) => void;
    onRemove: (userId: string) => void;
}

export const ProjectMembersSection = ({ members, roles, onInvite, onChangeRole, onRemove }: Props) => {
    const [inviteOpened, { open: openInvite, close: closeInvite }] = useDisclosure(false);
    const [removeTarget, setRemoveTarget] = useState<ProjectMember | undefined>(undefined);

    const roleOptions = roles.map((r) => ({ value: r.id, label: r.name }));

    const handleConfirmRemove = () => {
        if (removeTarget) {
            onRemove(removeTarget.userId);
        }
        setRemoveTarget(undefined);
    };

    return (
        <>
            <Stack gap="md">
                <Group justify="space-between">
                    <Stack gap={0}>
                        <Title order={3}>Członkowie</Title>
                        <Text size="sm" c="dimmed">
                            Właściciel ma zawsze pełne uprawnienia. Pozostali członkowie mają uprawnienia swojej roli.
                        </Text>
                    </Stack>
                    <Button leftSection={<UserPlusIcon />} onClick={openInvite}>
                        Zaproś
                    </Button>
                </Group>

                <Table.ScrollContainer minWidth={600}>
                    <Table striped highlightOnHover withTableBorder verticalSpacing="sm">
                        <Table.Thead>
                            <Table.Tr>
                                <Table.Th>Użytkownik</Table.Th>
                                <Table.Th w={260}>Rola</Table.Th>
                                <Table.Th w={60} />
                            </Table.Tr>
                        </Table.Thead>
                        <Table.Tbody>
                            {members.map((member) => (
                                <Table.Tr key={member.userId}>
                                    <Table.Td>
                                        <Stack gap={0}>
                                            <Text fw={500}>{member.userName}</Text>
                                            <Text size="xs" c="dimmed">
                                                {member.email}
                                            </Text>
                                        </Stack>
                                    </Table.Td>
                                    <Table.Td>
                                        {member.isOwner ? (
                                            <Badge color="yellow" variant="light" leftSection={<CrownIcon />}>
                                                Właściciel
                                            </Badge>
                                        ) : (
                                            <Select
                                                aria-label={`Rola użytkownika ${member.userName}`}
                                                data={roleOptions}
                                                value={member.roleId}
                                                allowDeselect={false}
                                                onChange={(value) => value && onChangeRole(member.userId, value)}
                                            />
                                        )}
                                    </Table.Td>
                                    <Table.Td>
                                        {!member.isOwner && (
                                            <Tooltip label="Usuń z projektu">
                                                <ActionIcon
                                                    variant="light"
                                                    color="red"
                                                    size="lg"
                                                    radius="sm"
                                                    onClick={() => setRemoveTarget(member)}
                                                >
                                                    <TrashIcon style={{ width: '70%', height: '70%' }} />
                                                </ActionIcon>
                                            </Tooltip>
                                        )}
                                    </Table.Td>
                                </Table.Tr>
                            ))}
                        </Table.Tbody>
                    </Table>
                </Table.ScrollContainer>
            </Stack>

            <InviteMemberDialog roles={roles} opened={inviteOpened} onClose={closeInvite} onSubmit={onInvite} />

            <ConfirmDeleteDialog
                opened={Boolean(removeTarget)}
                title="Usuń członka"
                message={`Czy na pewno usunąć użytkownika ${removeTarget?.userName ?? ''} z projektu?`}
                onConfirm={handleConfirmRemove}
                onClose={() => setRemoveTarget(undefined)}
            />
        </>
    );
};
