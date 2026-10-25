import type { TaskPriority, TaskStatus } from '../../client';

export const TaskStatusValue = {
    Todo: 0,
    InProgress: 1,
    Done: 2,
} as const satisfies Record<string, TaskStatus>;

export const TaskPriorityValue = {
    Critical: 0,
    Urgent: 1,
    Normal: 2,
    BySometime: 3,
    InFuture: 4,
} as const satisfies Record<string, TaskPriority>;

export const taskStatusOptions: { value: TaskStatus; label: string; color: string }[] = [
    { value: TaskStatusValue.Todo, label: 'Do zrobienia', color: 'gray' },
    { value: TaskStatusValue.InProgress, label: 'W trakcie', color: 'blue' },
    { value: TaskStatusValue.Done, label: 'Zrobione', color: 'green' },
];

export const taskPriorityOptions: { value: TaskPriority; label: string; color: string }[] = [
    { value: TaskPriorityValue.Critical, label: 'Krytyczny', color: 'red' },
    { value: TaskPriorityValue.Urgent, label: 'Pilny', color: 'orange' },
    { value: TaskPriorityValue.Normal, label: 'Normalny', color: 'blue' },
    { value: TaskPriorityValue.BySometime, label: 'Kiedyś', color: 'teal' },
    { value: TaskPriorityValue.InFuture, label: 'W przyszłości', color: 'gray' },
];

export const getTaskStatus = (value: TaskStatus) =>
    taskStatusOptions.find((x) => x.value === value) ?? taskStatusOptions[0];

export const getTaskPriority = (value: TaskPriority) =>
    taskPriorityOptions.find((x) => x.value === value) ?? taskPriorityOptions[2];

/** Dane dla Mantine Select, który operuje na stringach. */
export const toSelectData = (options: { value: number; label: string }[]) =>
    options.map((x) => ({ value: String(x.value), label: x.label }));
