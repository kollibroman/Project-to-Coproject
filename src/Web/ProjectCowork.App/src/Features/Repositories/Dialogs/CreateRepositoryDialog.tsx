import { Alert, Button, Group, Modal, Stack, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useCreateRepository } from '../../../hooks';

interface Props {
    projectId: string;
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    name: string;
}

export const CreateRepositoryDialog = ({ projectId, opened, onClose }: Props) => {
    const createRepository = useCreateRepository();

    const form = useForm<FormValues>({
        initialValues: {
            name: '',
        },
        validate: {
            name: (value) => (value.trim().length > 0 ? null : 'Nazwa jest wymagana'),
        },
    });

    const handleClose = () => {
        form.reset();
        createRepository.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        createRepository.mutate(
            { body: { name: values.name.trim(), projectId } },
            {
                onSuccess: () => handleClose(),
            },
        );
    });

    return (
        <Modal opened={opened} onClose={handleClose} title="Nowe repozytorium" centered>
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {createRepository.isError && (
                        <Alert color="red" title="Błąd">
                            {createRepository.error instanceof Error
                                ? createRepository.error.message
                                : 'Nie udało się utworzyć repozytorium.'}
                        </Alert>
                    )}

                    <TextInput
                        label="Nazwa"
                        placeholder="Nazwa repozytorium"
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('name')}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit" loading={createRepository.isPending}>
                            Utwórz
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
