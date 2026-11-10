import {
    ActionIcon,
    Alert,
    Anchor,
    Badge,
    Breadcrumbs,
    Button,
    Group,
    Paper,
    Progress,
    SegmentedControl,
    SimpleGrid,
    Skeleton,
    Stack,
    Switch,
    Text,
    TextInput,
    Title,
    Tooltip,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { CalendarBlankIcon, MagnifyingGlassIcon, PencilSimpleIcon, PlusIcon, TrashIcon } from '@phosphor-icons/react';
import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { Link, Navigate, useNavigate, useParams, useSearchParams } from 'react-router';
import type { SprintDto, TaskDto, TaskStatus } from '../../../client';
import {
    useDeleteSprint,
    useDeleteTask,
    useEditTask,
    useProject,
    useSprint,
    useSprints,
    useSprintTasks,
} from '../../../hooks';
import { TaskPaper } from '../Components/TaskPaper.tsx';
import { TaskGroup } from '../Components/TaskGroup.tsx';
import { ConfirmDeleteDialog } from '../Dialogs/ConfirmDeleteDialog.tsx';
import { SprintDialog } from '../Dialogs/SprintDialog.tsx';
import { TaskDialog } from '../Dialogs/TaskDialog.tsx';
import { TaskStatusValue, buildStatusUpdate, taskPriorityOptions, taskStatusOptions } from '../taskEnums.ts';

type GroupBy = 'status' | 'priority' | 'none';

const groupByOptions: { value: GroupBy; label: string }[] = [
    { value: 'status', label: 'Status' },
    { value: 'priority', label: 'Priorytet' },
    { value: 'none', label: 'Bez grupowania' },
];

const parseGroupBy = (value: string | null): GroupBy =>
    value === 'priority' || value === 'none' ? value : 'status';

type DeleteTarget = { kind: 'sprint' } | { kind: 'task'; task: TaskDto };

type TaskGroupData = { key: string; label?: string; color?: string; tasks: TaskDto[] };

const byPriorityThenStatus = (a: TaskDto, b: TaskDto) => a.priority - b.priority || a.status - b.status;
const byStatusThenPriority = (a: TaskDto, b: TaskDto) => a.status - b.status || a.priority - b.priority;

const buildGroups = (tasks: TaskDto[], groupBy: GroupBy): TaskGroupData[] => {
    if (groupBy === 'none') {
        return [{ key: 'all', tasks: [...tasks].sort(byPriorityThenStatus) }];
    }

    const options = groupBy === 'status' ? taskStatusOptions : taskPriorityOptions;
    const field = groupBy === 'status' ? 'status' : 'priority';
    const sort = groupBy === 'status' ? byPriorityThenStatus : byStatusThenPriority;

    return options
        .map((option) => ({
            key: String(option.value),
            label: option.label,
            color: option.color,
            tasks: tasks.filter((task) => task[field] === option.value).sort(sort),
        }))
        .filter((group) => group.tasks.length > 0);
};

/** Opis terminu sprintu względem dzisiejszej daty. Daty z API są w formacie YYYY-MM-DD. */
const describeTimeline = (sprint: SprintDto) => {
    const today = dayjs().startOf('day');
    const start = dayjs(sprint.startDate);
    const end = dayjs(sprint.endDate);

    if (today.isBefore(start)) {
        return { label: `Startuje za ${start.diff(today, 'day')} dni`, color: 'gray', isActive: false };
    }
    if (today.isAfter(end)) {
        return { label: 'Zakończony', color: 'gray', isActive: false };
    }

    const left = end.diff(today, 'day');
    return {
        label: left === 0 ? 'Ostatni dzień' : `Pozostało ${left} dni`,
        color: left <= 2 ? 'orange' : 'green',
        isActive: true,
    };
};

export const SprintPage = () => {
    const { projectId = '', sprintId = '' } = useParams<{ projectId: string; sprintId: string }>();
    const [searchParams, setSearchParams] = useSearchParams();
    const navigate = useNavigate();

    const projectQuery = useProject(projectId);
    const sprintQuery = useSprint(sprintId);
    const sprintsQuery = useSprints(projectId);
    const tasksQuery = useSprintTasks(sprintId);

    const editTask = useEditTask();
    const deleteSprint = useDeleteSprint();
    const deleteTask = useDeleteTask();

    const [sprintOpened, { open: openSprint, close: closeSprint }] = useDisclosure(false);
    const [taskOpened, { open: openTask, close: closeTask }] = useDisclosure(false);
    const [selectedTask, setSelectedTask] = useState<TaskDto | undefined>(undefined);
    const [deleteTarget, setDeleteTarget] = useState<DeleteTarget | undefined>(undefined);
    const [updatingTaskId, setUpdatingTaskId] = useState<string | undefined>(undefined);
    const [search, setSearch] = useState('');
    const [hideDone, setHideDone] = useState(false);

    // Grupowanie trzymamy w URL, żeby przetrwało odświeżenie i dało się wysłać link.
    const groupBy = parseGroupBy(searchParams.get('group'));
    const setGroupBy = (value: GroupBy) =>
        setSearchParams(value === 'status' ? {} : { group: value }, { replace: true });

    const tasks = useMemo(() => tasksQuery.data ?? [], [tasksQuery.data]);

    const stats = useMemo(() => {
        const count = (status: TaskStatus) => tasks.filter((t) => t.status === status).length;
        return {
            total: tasks.length,
            todo: count(TaskStatusValue.Todo),
            inProgress: count(TaskStatusValue.InProgress),
            done: count(TaskStatusValue.Done),
        };
    }, [tasks]);

    const groups = useMemo(() => {
        const phrase = search.trim().toLowerCase();
        const visible = tasks.filter((task) => {
            if (hideDone && task.status === TaskStatusValue.Done) return false;
            if (!phrase) return true;
            return (
                task.name.toLowerCase().includes(phrase) ||
                task.description.toLowerCase().includes(phrase) ||
                `#${task.friendlyId}`.includes(phrase)
            );
        });
        return buildGroups(visible, groupBy);
    }, [tasks, search, hideDone, groupBy]);

    const sprints = useMemo(
        () => [...(sprintsQuery.data ?? [])].sort((a, b) => a.startDate.localeCompare(b.startDate)),
        [sprintsQuery.data],
    );

    if (sprintQuery.isError) {
        return <Navigate to="/NotFound" replace />;
    }

    if (sprintQuery.isLoading || !sprintQuery.data) {
        return <Skeleton height={240} />;
    }

    const sprint = sprintQuery.data;
    const timeline = describeTimeline(sprint);
    const projectUrl = `/project/${projectId}?tab=sprints`;
    const percent = (value: number) => (stats.total === 0 ? 0 : (value / stats.total) * 100);

    const handleCreateTask = () => {
        setSelectedTask(undefined);
        openTask();
    };

    const handleEditTask = (task: TaskDto) => {
        setSelectedTask(task);
        openTask();
    };

    const handleStatusChange = (task: TaskDto, status: TaskStatus) => {
        setUpdatingTaskId(task.id);
        editTask.mutate(buildStatusUpdate(task, status), {
            onSettled: () => setUpdatingTaskId(undefined),
        });
    };

    const closeDelete = () => {
        setDeleteTarget(undefined);
        deleteSprint.reset();
        deleteTask.reset();
    };

    const handleConfirmDelete = () => {
        if (deleteTarget?.kind === 'sprint') {
            deleteSprint.mutate(
                { path: { sprintId: sprint.id } },
                { onSuccess: () => navigate(projectUrl, { replace: true }) },
            );
        } else if (deleteTarget?.kind === 'task') {
            deleteTask.mutate({ path: { taskId: deleteTarget.task.id } }, { onSuccess: closeDelete });
        }
    };

    const error = tasksQuery.error ?? (editTask.isError ? editTask.error : null);
    const hasFilters = search.trim().length > 0 || hideDone;

    return (
        <>
            <Stack gap="lg">
                <Breadcrumbs>
                    <Anchor component={Link} to="/projects" size="sm">
                        Projekty
                    </Anchor>
                    <Anchor component={Link} to={projectUrl} size="sm">
                        {projectQuery.data?.name ?? 'Projekt'}
                    </Anchor>
                    <Text size="sm">{sprint.name}</Text>
                </Breadcrumbs>

                <Group justify="space-between" align="flex-start">
                    <Stack gap={6}>
                        <Group gap="sm">
                            <Title order={1}>{sprint.name}</Title>
                            <Badge color={timeline.color} variant="light" size="lg">
                                {timeline.label}
                            </Badge>
                        </Group>
                        <Group gap={6} c="dimmed">
                            <CalendarBlankIcon />
                            <Text size="sm">
                                {dayjs(sprint.startDate).format('DD.MM.YYYY')} – {dayjs(sprint.endDate).format('DD.MM.YYYY')}
                            </Text>
                        </Group>
                    </Stack>

                    <Group gap="xs">
                        <Tooltip label="Edytuj sprint">
                            <ActionIcon variant="light" size="lg" radius="sm" onClick={openSprint}>
                                <PencilSimpleIcon style={{ width: '70%', height: '70%' }} />
                            </ActionIcon>
                        </Tooltip>
                        <Tooltip label="Usuń sprint">
                            <ActionIcon
                                variant="light"
                                color="red"
                                size="lg"
                                radius="sm"
                                onClick={() => setDeleteTarget({ kind: 'sprint' })}
                            >
                                <TrashIcon style={{ width: '70%', height: '70%' }} />
                            </ActionIcon>
                        </Tooltip>
                        <Button leftSection={<PlusIcon />} onClick={handleCreateTask}>
                            Nowe zadanie
                        </Button>
                    </Group>
                </Group>

                <Paper withBorder radius="sm" p="md">
                    <Stack gap="md">
                        <SimpleGrid cols={{ base: 2, sm: 4 }}>
                            <Stat label="Wszystkie" value={stats.total} />
                            <Stat label="Do zrobienia" value={stats.todo} color="gray" />
                            <Stat label="W trakcie" value={stats.inProgress} color="blue" />
                            <Stat label="Zrobione" value={stats.done} color="green" />
                        </SimpleGrid>
                        <Stack gap={4}>
                            <Text size="xs" c="dimmed">
                                Postęp: {Math.round(percent(stats.done))}%
                            </Text>
                            <Progress.Root size="md">
                                <Progress.Section value={percent(stats.done)} color="green" />
                                <Progress.Section value={percent(stats.inProgress)} color="blue" />
                            </Progress.Root>
                        </Stack>
                    </Stack>
                </Paper>

                <Group justify="space-between" align="flex-end">
                    <Group gap="md" align="flex-end">
                        <TextInput
                            w={260}
                            placeholder="Szukaj zadania lub #numer"
                            leftSection={<MagnifyingGlassIcon />}
                            value={search}
                            onChange={(event) => setSearch(event.currentTarget.value)}
                        />
                        <Switch
                            label="Ukryj ukończone"
                            checked={hideDone}
                            onChange={(event) => setHideDone(event.currentTarget.checked)}
                            mb={8}
                        />
                    </Group>
                    <Stack gap={4}>
                        <Text size="xs" c="dimmed">
                            Grupuj według
                        </Text>
                        <SegmentedControl
                            value={groupBy}
                            onChange={(value) => setGroupBy(value as GroupBy)}
                            data={groupByOptions}
                        />
                    </Stack>
                </Group>

                {error && (
                    <Alert color="red" title="Błąd">
                        {error instanceof Error ? error.message : 'Nie udało się pobrać lub zapisać zadań.'}
                    </Alert>
                )}

                {tasksQuery.isLoading && <Skeleton height={120} />}

                {!tasksQuery.isLoading && stats.total === 0 && (
                    <Text c="dimmed">W tym sprincie nie ma jeszcze zadań.</Text>
                )}

                {!tasksQuery.isLoading && stats.total > 0 && groups.length === 0 && hasFilters && (
                    <Text c="dimmed">Żadne zadanie nie pasuje do filtrów.</Text>
                )}

                {groups.map((group) => (
                    <TaskGroup key={group.key} label={group.label} color={group.color} count={group.tasks.length}>
                        {group.tasks.map((task) => (
                            <TaskPaper
                                key={task.id}
                                task={task}
                                hidePriority={groupBy === 'priority'}
                                onEdit={handleEditTask}
                                onDelete={(t) => setDeleteTarget({ kind: 'task', task: t })}
                                onStatusChange={handleStatusChange}
                                updating={updatingTaskId === task.id}
                            />
                        ))}
                    </TaskGroup>
                ))}
            </Stack>

            <SprintDialog projectId={projectId} sprint={sprint} opened={sprintOpened} onClose={closeSprint} />
            <TaskDialog
                projectId={projectId}
                sprints={sprints}
                task={selectedTask}
                defaultSprintId={sprint.id}
                opened={taskOpened}
                onClose={closeTask}
            />

            <ConfirmDeleteDialog
                opened={Boolean(deleteTarget)}
                title={deleteTarget?.kind === 'sprint' ? 'Usuń sprint' : 'Usuń zadanie'}
                message={
                    deleteTarget?.kind === 'task'
                        ? `Czy na pewno usunąć zadanie „${deleteTarget.task.name}”?`
                        : `Czy na pewno usunąć sprint „${sprint.name}”? Jego zadania wrócą do backlogu.`
                }
                loading={deleteSprint.isPending || deleteTask.isPending}
                error={deleteTarget?.kind === 'sprint' ? deleteSprint.error : deleteTask.error}
                onConfirm={handleConfirmDelete}
                onClose={closeDelete}
            />
        </>
    );
};

const Stat = ({ label, value, color }: { label: string; value: number; color?: string }) => (
    <Stack gap={0}>
        <Text size="xs" c="dimmed" tt="uppercase" fw={600}>
            {label}
        </Text>
        <Text size="xl" fw={700} c={color}>
            {value}
        </Text>
    </Stack>
);
