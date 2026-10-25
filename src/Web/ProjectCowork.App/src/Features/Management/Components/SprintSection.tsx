import { ActionIcon, Badge, Group, Paper, Progress, Stack, Text, Title, Tooltip } from '@mantine/core';
import { PencilSimpleIcon, PlusIcon, TrashIcon } from '@phosphor-icons/react';
import type { ReactNode } from 'react';
import type { SprintDto, TaskDto } from '../../../client';
import { TaskStatusValue } from '../taskEnums.ts';

interface Props {
    /** Brak sprintu oznacza sekcję backlogu. */
    sprint?: SprintDto;
    tasks: TaskDto[];
    isActive?: boolean;
    onAddTask: () => void;
    onEdit?: () => void;
    onDelete?: () => void;
    children: ReactNode;
}

export const SprintSection = ({ sprint, tasks, isActive, onAddTask, onEdit, onDelete, children }: Props) => {
    const done = tasks.filter((t) => t.status === TaskStatusValue.Done).length;
    const progress = tasks.length === 0 ? 0 : Math.round((done / tasks.length) * 100);

    return (
        <Paper withBorder shadow="md" radius="sm" p="xl">
            <Stack gap="md">
                <Group justify="space-between" align="flex-start" wrap="nowrap">
                    <Stack gap={4}>
                        <Group gap="sm">
                            <Title order={3}>{sprint ? sprint.name : 'Backlog'}</Title>
                            {isActive && (
                                <Badge color="green" variant="light">
                                    Aktywny
                                </Badge>
                            )}
                        </Group>
                        <Text size="sm" c="dimmed">
                            {sprint
                                ? `${sprint.startDate} – ${sprint.endDate}`
                                : 'Zadania nieprzypisane do żadnego sprintu'}
                        </Text>
                    </Stack>

                    <Group gap="xs" wrap="nowrap">
                        <Tooltip label="Dodaj zadanie">
                            <ActionIcon variant="light" size="lg" radius="sm" onClick={onAddTask}>
                                <PlusIcon style={{ width: '70%', height: '70%' }} />
                            </ActionIcon>
                        </Tooltip>
                        {onEdit && (
                            <Tooltip label="Edytuj sprint">
                                <ActionIcon variant="light" size="lg" radius="sm" onClick={onEdit}>
                                    <PencilSimpleIcon style={{ width: '70%', height: '70%' }} />
                                </ActionIcon>
                            </Tooltip>
                        )}
                        {onDelete && (
                            <Tooltip label="Usuń sprint">
                                <ActionIcon variant="light" color="red" size="lg" radius="sm" onClick={onDelete}>
                                    <TrashIcon style={{ width: '70%', height: '70%' }} />
                                </ActionIcon>
                            </Tooltip>
                        )}
                    </Group>
                </Group>

                {sprint && tasks.length > 0 && (
                    <Stack gap={4}>
                        <Text size="xs" c="dimmed">
                            Ukończone: {done}/{tasks.length}
                        </Text>
                        <Progress value={progress} size="sm" />
                    </Stack>
                )}

                {tasks.length === 0 ? <Text c="dimmed" size="sm">Brak zadań.</Text> : children}
            </Stack>
        </Paper>
    );
};
