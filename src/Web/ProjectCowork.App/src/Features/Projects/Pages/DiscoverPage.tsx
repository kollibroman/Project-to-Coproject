import { ActionIcon, Alert, Group, Loader, Paper, Stack, Text, Title } from '@mantine/core';
import { ArrowRightIcon } from '@phosphor-icons/react';
import { Link } from 'react-router';
import { useDiscoverProjects } from '../../../hooks';
import type { ProjectModel } from "../../../client";

export const DiscoverPage = () => {
    const { data, isLoading, isError, error } = useDiscoverProjects();

    return (
        <Stack gap="md">
            <Stack gap="xs">
                <Title order={1}>Odkryj</Title>
                <Text c="dimmed">
                    Projekty na platformie, do których nie należysz. Wejdź, aby zobaczyć ich oferty.
                </Text>
            </Stack>

            {isLoading && <Loader />}

            {isError && (
                <Alert color="red" title="Błąd">
                    {error instanceof Error ? error.message : 'Nie udało się pobrać projektów.'}
                </Alert>
            )}

            {!isLoading && !isError && (!data || data.length === 0) && (
                <Text c="dimmed">Brak projektów do odkrycia.</Text>
            )}

            {!isLoading &&
                !isError &&
                data?.map((project: ProjectModel) => (
                    <Paper key={project.id} withBorder shadow="md" radius="sm" p="xl">
                        <Group justify="space-between" align="flex-start" wrap="nowrap">
                            <Stack gap={4}>
                                <Title order={3}>{project.name}</Title>
                                <Text size="sm" c="dimmed">
                                    {project.description}
                                </Text>
                            </Stack>
                            <ActionIcon
                                variant="filled"
                                size="lg"
                                radius="sm"
                                component={Link}
                                to={`/project/${project.id}`}
                            >
                                <ArrowRightIcon style={{ width: '70%', height: '70%' }} />
                            </ActionIcon>
                        </Group>
                    </Paper>
                ))}
        </Stack>
    );
};
