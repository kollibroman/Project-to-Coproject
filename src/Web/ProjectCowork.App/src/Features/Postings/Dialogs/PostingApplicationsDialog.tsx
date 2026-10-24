import { Alert, Loader, Modal, Paper, Stack, Text } from '@mantine/core';
import { useEffect } from 'react';
import { useGetPostingApplications } from '../../../hooks';
import type { ProjectPostingDto } from '../../../client';

interface Props {
    posting?: ProjectPostingDto;
    opened: boolean;
    onClose: () => void;
}

export const PostingApplicationsDialog = ({ posting, opened, onClose }: Props) => {
    const applications = useGetPostingApplications();
    const { mutate, reset, data, isPending, isError, error } = applications;

    useEffect(() => {
        if (opened && posting) {
            mutate({ path: { projectPostingId: posting.id } });
        }
        if (!opened) {
            reset();
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [opened, posting?.id]);

    return (
        <Modal opened={opened} onClose={onClose} title="Kandydaci" centered size="lg">
            <Stack gap="md">
                {isPending && <Loader />}

                {isError && (
                    <Alert color="red" title="Błąd">
                        {error instanceof Error
                            ? error.message
                            : 'Nie udało się pobrać kandydatów.'}
                    </Alert>
                )}

                {!isPending && !isError && (!data || data.length === 0) && (
                    <Text c="dimmed">Brak kandydatów.</Text>
                )}

                {!isPending &&
                    !isError &&
                    data?.map((application) => (
                        <Paper key={application.id} withBorder radius="sm" p="md">
                            <Stack gap={4}>
                                <Text fw={600} size="sm">
                                    Użytkownik: {application.userEmail ?? application.userId}
                                </Text>
                                <Text size="sm">{application.description}</Text>
                            </Stack>
                        </Paper>
                    ))}
            </Stack>
        </Modal>
    );
};
