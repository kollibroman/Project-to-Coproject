import { ActionIcon, Badge, Group, Paper, Select, Stack, Text, Tooltip } from '@mantine/core';
import { PencilSimpleIcon, TrashIcon } from '@phosphor-icons/react';
import type { TaskDto, TaskStatus } from '../../../client';
import { TaskStatusValue, getTaskPriority, taskStatusOptions, toSelectData } from '../taskEnums.ts';

interface Props {
    task: TaskDto;
    onEdit: (task: TaskDto) => void;
    onDelete: (task: TaskDto) => void;
    onStatusChange: (task: TaskDto, status: TaskStatus) => void;
    updating?: boolean;
}

export const TaskPaper = ({ task, onEdit, onDelete, onStatusChange, updating }: Props) => {
    const priority = getTaskPriority(task.priority);
    const isDone = task.status === TaskStatusValue.Done;

    return (
        <Paper withBorder radius="sm" p="md">
            <Group justify="space-between" align="flex-start" wrap="nowrap">
                <Stack gap={4} style={{ minWidth: 0 }}>
                    <Group gap="xs">
                        <Text fw={600} td={isDone ? 'line-through' : undefined}>
                            {task.name}
                        </Text>
                        <Badge color={priority.color} variant="light" size="sm">
                            {priority.label}
                        </Badge>
                    </Group>
                    {task.description && (
                        <Text size="sm" c="dimmed" lineClamp={2}>
                            {task.description}
                        </Text>
                    )}
                    {isDone && task.finishedDate && (
                        <Text size="xs" c="dimmed">
                            Ukończono: {task.finishedDate}
                        </Text>
                    )}
                </Stack>

                <Group gap="xs" wrap="nowrap">
                    <Select
                        size="xs"
                        w={140}
                        aria-label="Status"
                        data={toSelectData(taskStatusOptions)}
                        value={String(task.status)}
                        allowDeselect={false}
                        disabled={updating}
                        onChange={(value) => {
                            if (value !== null && Number(value) !== task.status) {
                                onStatusChange(task, Number(value));
                            }
                        }}
                    />
                    <Tooltip label="Edytuj">
                        <ActionIcon variant="light" size="lg" radius="sm" onClick={() => onEdit(task)}>
                            <PencilSimpleIcon style={{ width: '70%', height: '70%' }} />
                        </ActionIcon>
                    </Tooltip>
                    <Tooltip label="Usuń">
                        <ActionIcon
                            variant="light"
                            color="red"
                            size="lg"
                            radius="sm"
                            onClick={() => onDelete(task)}
                        >
                            <TrashIcon style={{ width: '70%', height: '70%' }} />
                        </ActionIcon>
                    </Tooltip>
                </Group>
            </Group>
        </Paper>
    );
};
