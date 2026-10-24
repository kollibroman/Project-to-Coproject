import { Alert, Button, Group, Modal, Stack, Textarea, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useCreateProject } from '../../../hooks';
import type { CreateProjectRequest } from '../../../client';

interface Props {
    opened: boolean;
    onClose: () => void;
}

export const CreateProjectDialog = ({ opened, onClose }: Props) => {
    const createProject = useCreateProject();

    const form = useForm<CreateProjectRequest>({
        initialValues: {
            name: '',
            description: '',
        },
        validate: {
            name: (value) => (value.trim().length > 0 ? null : 'Nazwa jest wymagana'),
        },
    });

    const handleClose = () => {
        form.reset();
        createProject.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        createProject.mutate(
            { body: values },
            {
                onSuccess: () => handleClose(),
            },
        );
    });

    return (
        <Modal opened={opened} onClose={handleClose} title="Nowy projekt" centered>
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {createProject.isError && (
                        <Alert color="red" title="Błąd">
                            {createProject.error instanceof Error
                                ? createProject.error.message
                                : 'Nie udało się utworzyć projektu.'}
                        </Alert>
                    )}

                    <TextInput
                        label="Nazwa"
                        placeholder="Nazwa projektu"
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('name')}
                    />

                    <Textarea
                        label="Opis"
                        placeholder="Krótki opis projektu"
                        autosize
                        minRows={3}
                        {...form.getInputProps('description')}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit" loading={createProject.isPending}>
                            Utwórz
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
