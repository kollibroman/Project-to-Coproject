import { Alert, Button, Group, Loader, Paper, Stack, Text, Title } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useState } from 'react';
import { useActivePostings } from '../../../hooks';
import type { ProjectPostingDto } from '../../../client';
import { ApplyForPostingDialog } from '../Dialogs/ApplyForPostingDialog.tsx';

export const PostingsPage = () => {
    const { data, isLoading, isError, error } = useActivePostings();
    const [opened, { open, close }] = useDisclosure(false);
    const [selected, setSelected] = useState<ProjectPostingDto | undefined>(undefined);

    const handleApply = (posting: ProjectPostingDto) => {
        setSelected(posting);
        open();
    };

    return (
        <Stack gap="md">
            <Stack gap="xs">
                <Title order={1}>Oferty</Title>
                <Text c="dimmed">Otwarte oferty, na które możesz aplikować.</Text>
            </Stack>

            {isLoading && <Loader />}

            {isError && (
                <Alert color="red" title="Błąd">
                    {error instanceof Error ? error.message : 'Nie udało się pobrać ofert.'}
                </Alert>
            )}

            {!isLoading && !isError && (!data || data.length === 0) && (
                <Text c="dimmed">Brak dostępnych ofert.</Text>
            )}

            {!isLoading &&
                !isError &&
                data?.map((posting) => (
                    <Paper key={posting.id} withBorder shadow="md" radius="sm" p="xl">
                        <Group justify="space-between" align="flex-start" wrap="nowrap">
                            <Stack gap={4}>
                                <Title order={3}>{posting.jobDescription}</Title>
                                <Text size="sm" c="dimmed">
                                    {posting.projectDescription}
                                </Text>
                            </Stack>
                            <Button onClick={() => handleApply(posting)}>Aplikuj</Button>
                        </Group>
                    </Paper>
                ))}

            <ApplyForPostingDialog posting={selected} opened={opened} onClose={close} />
        </Stack>
    );
};
