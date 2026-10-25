import { Alert, Button, Group, Modal, Stack, Text } from '@mantine/core';

interface Props {
    opened: boolean;
    title: string;
    message: string;
    loading?: boolean;
    error?: unknown;
    onConfirm: () => void;
    onClose: () => void;
}

export const ConfirmDeleteDialog = ({ opened, title, message, loading, error, onConfirm, onClose }: Props) => (
    <Modal opened={opened} onClose={onClose} title={title} centered>
        <Stack gap="md">
            {Boolean(error) && (
                <Alert color="red" title="Błąd">
                    {error instanceof Error ? error.message : 'Nie udało się usunąć.'}
                </Alert>
            )}

            <Text>{message}</Text>

            <Group justify="flex-end">
                <Button variant="default" onClick={onClose}>
                    Anuluj
                </Button>
                <Button color="red" loading={loading} onClick={onConfirm}>
                    Usuń
                </Button>
            </Group>
        </Stack>
    </Modal>
);
