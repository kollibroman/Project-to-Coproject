import { ActionIcon, Anchor, Badge, Group, Paper, Progress, Stack, Text, Title, Tooltip } from '@mantine/core';
import { ArrowRightIcon, PencilSimpleIcon, PlusIcon, TrashIcon } from '@phosphor-icons/react';
import type { ReactNode } from 'react';
import { Link } from 'react-router';
import type { SprintDto, TaskDto } from '../../../client';
import { TaskStatusValue, taskStatusOptions } from '../taskEnums.ts';

interface Props {
    /** Brak sprintu oznacza sekcję backlogu. */
    sprint?: SprintDto;
    tasks: TaskDto[];
    isActive?: boolean;
    /** Adres strony sprintu. Gdy podany, nazwa sprintu staje się linkiem. */
    to?: string;
    onAddTask: () => void;
    onEdit?: () => void;
    onDelete?: () => void;
    children?: ReactNode;
}

const formatTaskCount = (count: number) => {
    const lastTwo = count % 100;
    const last = count % 10;
    if (count === 1) return '1 zadanie';
    if (last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14)) return `${count} zadania`;
    return `${count} zadań`;
};

export const SprintSection = ({ sprint, tasks, isActive, to, onAddTask, onEdit, onDelete, children }: Props) => {
    const done = tasks.filter((t) => t.status === TaskStatusValue.Done).length;
    const progress = tasks.length === 0 ? 0 : Math.round((done / tasks.length) * 100);

    return (
        <Paper withBorder shadow="md" radius="sm" p="xl">
            <Stack gap="md">
                <Group justify="space-between" align="flex-start" wrap="nowrap">
                    <Stack gap={4}>
                        <Group gap="sm">
                            {sprint && to ? (
                                <Anchor component={Link} to={to} c="inherit" underline="hover">
                                    <Title order={3}>{sprint.name}</Title>
                                </Anchor>
                            ) : (
                                <Title order={3}>{sprint ? sprint.name : 'Backlog'}</Title>
                            )}
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
                        {to && (
                            <Tooltip label="Otwórz sprint">
                                <ActionIcon component={Link} to={to} variant="light" size="lg" radius="sm">
                                    <ArrowRightIcon style={{ width: '70%', height: '70%' }} />
                                </ActionIcon>
                            </Tooltip>
                        )}
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

                {tasks.length === 0 && <Text c="dimmed" size="sm">Brak zadań.</Text>}

                {sprint && tasks.length > 0 && (
                    <Stack gap="xs">
                        <Group gap="xs">
                            <Text fw={600}>{formatTaskCount(tasks.length)}</Text>
                            {taskStatusOptions.map((option) => (
                                <Badge key={option.value} color={option.color} variant="light">
                                    {option.label}: {tasks.filter((t) => t.status === option.value).length}
                                </Badge>
                            ))}
                        </Group>
                        <Stack gap={4}>
                            <Text size="xs" c="dimmed">
                                Ukończone: {done}/{tasks.length}
                            </Text>
                            <Progress value={progress} size="sm" />
                        </Stack>
                    </Stack>
                )}

                {tasks.length > 0 && children}
            </Stack>
        </Paper>
    );
};
