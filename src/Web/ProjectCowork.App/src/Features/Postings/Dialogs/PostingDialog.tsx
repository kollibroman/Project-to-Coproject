import { Alert, Button, Group, Modal, Stack, Textarea } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useEffect } from 'react';
import { useAddPosting, useUpdatePosting } from '../../../hooks';
import type { ProjectPostingDto } from '../../../client';

interface Props {
    projectId: string;
    posting?: ProjectPostingDto;
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    jobDescription: string;
    projectDescription: string;
}

export const PostingDialog = ({ projectId, posting, opened, onClose }: Props) => {
    const isEdit = Boolean(posting);

    const addPosting = useAddPosting();
    const updatePosting = useUpdatePosting();
    const mutation = isEdit ? updatePosting : addPosting;

    const form = useForm<FormValues>({
        initialValues: {
            jobDescription: '',
            projectDescription: '',
        },
        validate: {
            jobDescription: (value) =>
                value.trim().length > 0 ? null : 'Opis stanowiska jest wymagany',
            projectDescription: (value) =>
                value.trim().length > 0 ? null : 'Opis projektu jest wymagany',
        },
    });

    useEffect(() => {
        if (opened) {
            form.setValues({
                jobDescription: posting?.jobDescription ?? '',
                projectDescription: posting?.projectDescription ?? '',
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [opened, posting]);

    const handleClose = () => {
        form.reset();
        addPosting.reset();
        updatePosting.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        if (isEdit && posting) {
            updatePosting.mutate(
                {
                    body: {
                        projectPostingId: posting.id,
                        jobDescription: values.jobDescription.trim(),
                        projectDescription: values.projectDescription.trim(),
                        isActive: true,
                    },
                },
                { onSuccess: () => handleClose() },
            );
        } else {
            addPosting.mutate(
                {
                    body: {
                        projectId,
                        jobDescription: values.jobDescription.trim(),
                        projectDescription: values.projectDescription.trim(),
                    },
                },
                { onSuccess: () => handleClose() },
            );
        }
    });

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={isEdit ? 'Edytuj ogłoszenie' : 'Nowe ogłoszenie'}
            centered
        >
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {mutation.isError && (
                        <Alert color="red" title="Błąd">
                            {mutation.error instanceof Error
                                ? mutation.error.message
                                : 'Nie udało się zapisać ogłoszenia.'}
                        </Alert>
                    )}

                    <Textarea
                        label="Opis stanowiska"
                        placeholder="Czego szukasz, jakie umiejętności"
                        withAsterisk
                        autosize
                        minRows={3}
                        data-autofocus
                        {...form.getInputProps('jobDescription')}
                    />

                    <Textarea
                        label="Opis projektu"
                        placeholder="Krótki opis projektu"
                        withAsterisk
                        autosize
                        minRows={3}
                        {...form.getInputProps('projectDescription')}
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
