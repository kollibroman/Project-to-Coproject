import { ActionIcon, Badge, Group, Paper, Stack, Text, Title, Tooltip } from '@mantine/core';
import { CheckCircleIcon, PencilSimpleIcon, ProhibitIcon, UsersIcon } from '@phosphor-icons/react';
import type { ProjectPostingDto } from '../../../client';

interface Props {
    posting: ProjectPostingDto;
    onEdit: (posting: ProjectPostingDto) => void;
    onToggleActive: (posting: ProjectPostingDto) => void;
    onShowApplications: (posting: ProjectPostingDto) => void;
    toggling?: boolean;
}

export const PostingPaper = ({
    posting,
    onEdit,
    onToggleActive,
    onShowApplications,
    toggling,
}: Props) => {
    return (
        <Paper withBorder shadow="md" radius="sm" p="xl">
            <Group justify="space-between" align="flex-start" wrap="nowrap">
                <Stack gap="xs">
                    <Group gap="sm">
                        <Title order={3}>{posting.jobDescription}</Title>
                        <Badge color={posting.isActive ? 'green' : 'gray'} variant="light">
                            {posting.isActive ? 'Aktywne' : 'Nieaktywne'}
                        </Badge>
                    </Group>
                    <Text size="sm" c="dimmed">
                        {posting.projectDescription}
                    </Text>
                </Stack>

                <Group gap="xs" wrap="nowrap">
                    <Tooltip label="Kandydaci">
                        <ActionIcon
                            variant="light"
                            size="lg"
                            radius="sm"
                            onClick={() => onShowApplications(posting)}
                        >
                            <UsersIcon style={{ width: '70%', height: '70%' }} />
                        </ActionIcon>
                    </Tooltip>
                    <Tooltip label="Edytuj">
                        <ActionIcon
                            variant="light"
                            size="lg"
                            radius="sm"
                            onClick={() => onEdit(posting)}
                        >
                            <PencilSimpleIcon style={{ width: '70%', height: '70%' }} />
                        </ActionIcon>
                    </Tooltip>
                    <Tooltip label={posting.isActive ? 'Dezaktywuj' : 'Aktywuj'}>
                        <ActionIcon
                            variant="light"
                            color={posting.isActive ? 'red' : 'green'}
                            size="lg"
                            radius="sm"
                            loading={toggling}
                            onClick={() => onToggleActive(posting)}
                        >
                            {posting.isActive ? (
                                <ProhibitIcon style={{ width: '70%', height: '70%' }} />
                            ) : (
                                <CheckCircleIcon style={{ width: '70%', height: '70%' }} />
                            )}
                        </ActionIcon>
                    </Tooltip>
                </Group>
            </Group>
        </Paper>
    );
};
