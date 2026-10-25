import { Alert, Button, Group, Modal, Select, Stack, Textarea, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useEffect, useMemo } from 'react';
import { useCreateTask, useEditTask } from '../../../hooks';
import type { SprintDto, TaskDto } from '../../../client';
import {
    TaskPriorityValue,
    TaskStatusValue,
    taskPriorityOptions,
    taskStatusOptions,
    toSelectData,
} from '../taskEnums.ts';

const BACKLOG = 'backlog';

interface Props {
    projectId: string;
    sprints: SprintDto[];
    task?: TaskDto;
    /** Sprint zaznaczony domyślnie przy tworzeniu zadania (np. z poziomu sekcji sprintu). */
    defaultSprintId?: string | null;
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    name: string;
    description: string;
    status: string;
    priority: string;
    sprintId: string;
}

export const TaskDialog = ({ projectId, sprints, task, defaultSprintId, opened, onClose }: Props) => {
    const isEdit = Boolean(task);

    const createTask = useCreateTask();
    const editTask = useEditTask();
    const mutation = isEdit ? editTask : createTask;

    const sprintData = useMemo(
        () => [
            { value: BACKLOG, label: 'Backlog (bez sprintu)' },
            ...sprints.map((s) => ({ value: s.id, label: s.name })),
        ],
        [sprints],
    );

    const form = useForm<FormValues>({
        initialValues: {
            name: '',
            description: '',
            status: String(TaskStatusValue.Todo),
            priority: String(TaskPriorityValue.Normal),
            sprintId: BACKLOG,
        },
        validate: {
            name: (value) => (value.trim().length > 0 ? null : 'Nazwa zadania jest wymagana'),
        },
    });

    useEffect(() => {
        if (opened) {
            form.setValues({
                name: task?.name ?? '',
                description: task?.description ?? '',
                status: String(task?.status ?? TaskStatusValue.Todo),
                priority: String(task?.priority ?? TaskPriorityValue.Normal),
                sprintId: (task ? task.sprintId : defaultSprintId) ?? BACKLOG,
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [opened, task, defaultSprintId]);

    const handleClose = () => {
        form.reset();
        createTask.reset();
        editTask.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        const fields = {
            name: values.name.trim(),
            description: values.description.trim(),
            status: Number(values.status),
            priority: Number(values.priority),
            sprintId: values.sprintId === BACKLOG ? null : values.sprintId,
        };

        if (isEdit && task) {
            editTask.mutate(
                {
                    path: { taskId: task.id },
                    // API nie ma jeszcze listy członków projektu, więc przypisania nie da się
                    // zmienić z UI. Przekazujemy obecną wartość, bo null oznacza "odepnij".
                    body: { ...fields, assignedUserId: task.assignedUserId ?? null },
                },
                { onSuccess: () => handleClose() },
            );
        } else {
            createTask.mutate(
                { body: { ...fields, projectId, assignedUserId: null } },
                { onSuccess: () => handleClose() },
            );
        }
    });

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={isEdit ? 'Edytuj zadanie' : 'Nowe zadanie'}
            centered
        >
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {mutation.isError && (
                        <Alert color="red" title="Błąd">
                            {mutation.error instanceof Error
                                ? mutation.error.message
                                : 'Nie udało się zapisać zadania.'}
                        </Alert>
                    )}

                    <TextInput
                        label="Nazwa"
                        placeholder="Co trzeba zrobić"
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('name')}
                    />

                    <Textarea
                        label="Opis"
                        placeholder="Szczegóły zadania"
                        autosize
                        minRows={3}
                        {...form.getInputProps('description')}
                    />

                    <Group grow>
                        <Select
                            label="Status"
                            data={toSelectData(taskStatusOptions)}
                            allowDeselect={false}
                            {...form.getInputProps('status')}
                        />
                        <Select
                            label="Priorytet"
                            data={toSelectData(taskPriorityOptions)}
                            allowDeselect={false}
                            {...form.getInputProps('priority')}
                        />
                    </Group>

                    <Select
                        label="Sprint"
                        data={sprintData}
                        allowDeselect={false}
                        {...form.getInputProps('sprintId')}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit" loading={mutation.isPending}>
                            {isEdit ? 'Zapisz' : 'Utwórz'}
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
