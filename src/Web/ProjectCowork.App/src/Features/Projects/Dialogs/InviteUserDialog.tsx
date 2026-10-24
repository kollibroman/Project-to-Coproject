import { Alert, Button, Group, Modal, Stack, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useInviteUser } from '../../../hooks';

interface Props {
    projectId: string;
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    email: string;
}

export const InviteUserDialog = ({ projectId, opened, onClose }: Props) => {
    const inviteUser = useInviteUser();

    const form = useForm<FormValues>({
        initialValues: {
            email: '',
        },
        validate: {
            email: (value) =>
                /^\S+@\S+\.\S+$/.test(value.trim()) ? null : 'Podaj poprawny adres e-mail',
        },
    });

    const handleClose = () => {
        form.reset();
        inviteUser.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        inviteUser.mutate(
            {
                path: { projectId },
                body: { projectId, email: values.email.trim() },
            },
            {
                onSuccess: () => handleClose(),
            },
        );
    });

    return (
        <Modal opened={opened} onClose={handleClose} title="Zaproś użytkownika" centered>
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {inviteUser.isError && (
                        <Alert color="red" title="Błąd">
                            {inviteUser.error instanceof Error
                                ? inviteUser.error.message
                                : 'Nie udało się zaprosić użytkownika.'}
                        </Alert>
                    )}

                    <TextInput
                        label="Adres e-mail"
                        placeholder="uzytkownik@example.com"
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('email')}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit" loading={inviteUser.isPending}>
                            Zaproś
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
