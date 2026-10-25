import { Alert, Button, Group, Loader, Stack, Text } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { PlusIcon } from '@phosphor-icons/react';
import { useMemo, useState } from 'react';
import {
    useDeleteSprint,
    useDeleteTask,
    useEditTask,
    useProjectTasks,
    useSprints,
} from '../../../hooks';
import type { SprintDto, TaskDto, TaskStatus } from '../../../client';
import { SprintSection } from './SprintSection.tsx';
import { TaskPaper } from './TaskPaper.tsx';
import { SprintDialog } from '../Dialogs/SprintDialog.tsx';
import { TaskDialog } from '../Dialogs/TaskDialog.tsx';
import { ConfirmDeleteDialog } from '../Dialogs/ConfirmDeleteDialog.tsx';

type SprintsTabProps = {
    projectId: string | undefined;
};

type DeleteTarget = { kind: 'sprint'; sprint: SprintDto } | { kind: 'task'; task: TaskDto };

const today = () => new Date().toISOString().slice(0, 10);

export const SprintsTab = ({ projectId }: SprintsTabProps) => {
    const sprintsQuery = useSprints(projectId);
    const tasksQuery = useProjectTasks(projectId);

    const editTask = useEditTask();
    const deleteSprint = useDeleteSprint();
    const deleteTask = useDeleteTask();

    const [sprintOpened, { open: openSprint, close: closeSprint }] = useDisclosure(false);
    const [taskOpened, { open: openTask, close: closeTask }] = useDisclosure(false);

    const [selectedSprint, setSelectedSprint] = useState<SprintDto | undefined>(undefined);
    const [selectedTask, setSelectedTask] = useState<TaskDto | undefined>(undefined);
    const [defaultSprintId, setDefaultSprintId] = useState<string | null>(null);
    const [deleteTarget, setDeleteTarget] = useState<DeleteTarget | undefined>(undefined);
    const [updatingTaskId, setUpdatingTaskId] = useState<string | undefined>(undefined);

    const sprints = useMemo(
        () => [...(sprintsQuery.data ?? [])].sort((a, b) => a.startDate.localeCompare(b.startDate)),
        [sprintsQuery.data],
    );

    // Zadania pobieramy jednym zapytaniem dla projektu i grupujemy po sprintId.
    const tasksBySprint = useMemo(() => {
        const groups = new Map<string | null, TaskDto[]>();
        for (const task of tasksQuery.data ?? []) {
            const key = task.sprintId ?? null;
            groups.set(key, [...(groups.get(key) ?? []), task]);
        }
        for (const list of groups.values()) {
            list.sort((a, b) => a.priority - b.priority || a.status - b.status);
        }
        return groups;
    }, [tasksQuery.data]);

    const handleCreateSprint = () => {
        setSelectedSprint(undefined);
        openSprint();
    };

    const handleEditSprint = (sprint: SprintDto) => {
        setSelectedSprint(sprint);
        openSprint();
    };

    const handleCreateTask = (sprintId: string | null) => {
        setSelectedTask(undefined);
        setDefaultSprintId(sprintId);
        openTask();
    };

    const handleEditTask = (task: TaskDto) => {
        setSelectedTask(task);
        openTask();
    };

    const handleStatusChange = (task: TaskDto, status: TaskStatus) => {
        setUpdatingTaskId(task.id);
        editTask.mutate(
            {
                path: { taskId: task.id },
                body: {
                    name: task.name,
                    description: task.description,
                    priority: task.priority,
                    status,
                    sprintId: task.sprintId ?? null,
                    assignedUserId: task.assignedUserId ?? null,
                },
            },
            { onSettled: () => setUpdatingTaskId(undefined) },
        );
    };

    const closeDelete = () => {
        setDeleteTarget(undefined);
        deleteSprint.reset();
        deleteTask.reset();
    };

    const handleConfirmDelete = () => {
        if (!deleteTarget) {
            return;
        }
        if (deleteTarget.kind === 'sprint') {
            deleteSprint.mutate(
                { path: { sprintId: deleteTarget.sprint.id } },
                { onSuccess: () => closeDelete() },
            );
        } else {
            deleteTask.mutate(
                { path: { taskId: deleteTarget.task.id } },
                { onSuccess: () => closeDelete() },
            );
        }
    };

    const renderTasks = (tasks: TaskDto[]) =>
        tasks.map((task) => (
            <TaskPaper
                key={task.id}
                task={task}
                onEdit={handleEditTask}
                onDelete={(t) => setDeleteTarget({ kind: 'task', task: t })}
                onStatusChange={handleStatusChange}
                updating={updatingTaskId === task.id}
            />
        ));

    const isLoading = sprintsQuery.isLoading || tasksQuery.isLoading;
    const error = sprintsQuery.error ?? tasksQuery.error ?? (editTask.isError ? editTask.error : null);
    const backlog = tasksBySprint.get(null) ?? [];
    const now = today();

    return (
        <>
            <Stack gap="md">
                <Group justify="flex-end">
                    <Button variant="default" leftSection={<PlusIcon />} onClick={handleCreateSprint} disabled={!projectId}>
                        Nowy sprint
                    </Button>
                    <Button leftSection={<PlusIcon />} onClick={() => handleCreateTask(null)} disabled={!projectId}>
                        Nowe zadanie
                    </Button>
                </Group>

                {error && (
                    <Alert color="red" title="Błąd">
                        {error instanceof Error ? error.message : 'Nie udało się pobrać sprintów lub zadań.'}
                    </Alert>
                )}

                {isLoading && <Loader />}

                {!isLoading && sprints.length === 0 && (
                    <Text c="dimmed">Brak sprintów. Utwórz pierwszy sprint, aby planować zadania.</Text>
                )}

                {!isLoading &&
                    sprints.map((sprint) => {
                        const tasks = tasksBySprint.get(sprint.id) ?? [];
                        return (
                            <SprintSection
                                key={sprint.id}
                                sprint={sprint}
                                tasks={tasks}
                                isActive={sprint.startDate <= now && now <= sprint.endDate}
                                onAddTask={() => handleCreateTask(sprint.id)}
                                onEdit={() => handleEditSprint(sprint)}
                                onDelete={() => setDeleteTarget({ kind: 'sprint', sprint })}
                            >
                                {renderTasks(tasks)}
                            </SprintSection>
                        );
                    })}

                {!isLoading && (
                    <SprintSection tasks={backlog} onAddTask={() => handleCreateTask(null)}>
                        {renderTasks(backlog)}
                    </SprintSection>
                )}
            </Stack>

            {projectId && (
                <>
                    <SprintDialog
                        projectId={projectId}
                        sprint={selectedSprint}
                        opened={sprintOpened}
                        onClose={closeSprint}
                    />
                    <TaskDialog
                        projectId={projectId}
                        sprints={sprints}
                        task={selectedTask}
                        defaultSprintId={defaultSprintId}
                        opened={taskOpened}
                        onClose={closeTask}
                    />
                </>
            )}

            <ConfirmDeleteDialog
                opened={Boolean(deleteTarget)}
                title={deleteTarget?.kind === 'sprint' ? 'Usuń sprint' : 'Usuń zadanie'}
                message={
                    deleteTarget?.kind === 'sprint'
                        ? `Czy na pewno usunąć sprint „${deleteTarget.sprint.name}”? Jego zadania wrócą do backlogu.`
                        : `Czy na pewno usunąć zadanie „${deleteTarget?.task.name ?? ''}”?`
                }
                loading={deleteSprint.isPending || deleteTask.isPending}
                error={deleteTarget?.kind === 'sprint' ? deleteSprint.error : deleteTask.error}
                onConfirm={handleConfirmDelete}
                onClose={closeDelete}
            />
        </>
    );
};
