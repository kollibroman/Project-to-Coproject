import { Alert, Button, Group, Modal, Stack, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useEffect } from 'react';
import { useCreateSprint, useEditSprint } from '../../../hooks';
import type { SprintDto } from '../../../client';

interface Props {
    projectId: string;
    sprint?: SprintDto;
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    name: string;
    startDate: string;
    endDate: string;
}

export const SprintDialog = ({ projectId, sprint, opened, onClose }: Props) => {
    const isEdit = Boolean(sprint);

    const createSprint = useCreateSprint();
    const editSprint = useEditSprint();
    const mutation = isEdit ? editSprint : createSprint;

    const form = useForm<FormValues>({
        initialValues: {
            name: '',
            startDate: '',
            endDate: '',
        },
        validate: {
            name: (value) => (value.trim().length > 0 ? null : 'Nazwa sprintu jest wymagana'),
            startDate: (value) => (value ? null : 'Data rozpoczęcia jest wymagana'),
            // Daty są w formacie YYYY-MM-DD, więc porównanie stringów działa poprawnie.
            endDate: (value, values) => {
                if (!value) {
                    return 'Data zakończenia jest wymagana';
                }
                return values.startDate && value < values.startDate
                    ? 'Data zakończenia nie może być wcześniejsza niż data rozpoczęcia'
                    : null;
            },
        },
    });

    useEffect(() => {
        if (opened) {
            form.setValues({
                name: sprint?.name ?? '',
                startDate: sprint?.startDate ?? '',
                endDate: sprint?.endDate ?? '',
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [opened, sprint]);

    const handleClose = () => {
        form.reset();
        createSprint.reset();
        editSprint.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        const body = {
            name: values.name.trim(),
            startDate: values.startDate,
            endDate: values.endDate,
        };

        if (isEdit && sprint) {
            editSprint.mutate(
                { path: { sprintId: sprint.id }, body },
                { onSuccess: () => handleClose() },
            );
        } else {
            createSprint.mutate(
                { body: { ...body, projectId } },
                { onSuccess: () => handleClose() },
            );
        }
    });

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={isEdit ? 'Edytuj sprint' : 'Nowy sprint'}
            centered
        >
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {mutation.isError && (
                        <Alert color="red" title="Błąd">
                            {mutation.error instanceof Error
                                ? mutation.error.message
                                : 'Nie udało się zapisać sprintu.'}
                        </Alert>
                    )}

                    <TextInput
                        label="Nazwa"
                        placeholder="Sprint 1"
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('name')}
                    />

                    <Group grow>
                        <TextInput
                            type="date"
                            label="Początek"
                            withAsterisk
                            {...form.getInputProps('startDate')}
                        />
                        <TextInput
                            type="date"
                            label="Koniec"
                            withAsterisk
                            {...form.getInputProps('endDate')}
                        />
                    </Group>

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
