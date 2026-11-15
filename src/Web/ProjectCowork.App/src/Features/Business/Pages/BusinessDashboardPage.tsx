import {
    ActionIcon,
    Alert,
    Anchor,
    Card,
    Group,
    Loader,
    Paper,
    SimpleGrid,
    Stack,
    Text,
    ThemeIcon,
    Title,
} from '@mantine/core';
import { ArrowRightIcon, BuildingsIcon, CompassIcon } from '@phosphor-icons/react';
import { Link } from 'react-router';
import type { ComponentType } from 'react';
import { useDiscoverProjects } from '../../../hooks';
import type { ProjectModel } from '../../../client';

type DashboardTile = {
    title: string;
    description: string;
    to: string;
    icon: ComponentType<{ size?: number }>;
    color: string;
};

const tiles: DashboardTile[] = [
    {
        title: 'Odkryj projekty',
        description: 'Przeglądaj projekty studentów i skontaktuj się z ich zespołami.',
        to: '/discover',
        icon: CompassIcon,
        color: 'teal',
    },
    {
        title: 'Konto firmy',
        description: 'Dane firmy, ustawienia konta i wygląd aplikacji.',
        to: '/account',
        icon: BuildingsIcon,
        color: 'blue',
    },
];

const LATEST_PROJECTS_COUNT = 3;

export const BusinessDashboardPage = () => {
    const { data, isLoading, isError, error } = useDiscoverProjects();
    const latestProjects = data?.slice(0, LATEST_PROJECTS_COUNT) ?? [];

    return (
        <Stack gap="lg">
            <Stack gap="xs">
                <Title order={1}>Pulpit</Title>
                <Text c="dimmed">
                    Przeglądaj studenckie projekty i nawiązuj współpracę z ich zespołami.
                </Text>
            </Stack>

            <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
                {tiles.map((tile) => (
                    <Card
                        key={tile.to}
                        component={Link}
                        to={tile.to}
                        withBorder
                        shadow="sm"
                        radius="md"
                        padding="xl"
                    >
                        <Stack gap="sm">
                            <ThemeIcon variant="light" color={tile.color} size={48} radius="md">
                                <tile.icon size={28} />
                            </ThemeIcon>
                            <Title order={3}>{tile.title}</Title>
                            <Text size="sm" c="dimmed">
                                {tile.description}
                            </Text>
                        </Stack>
                    </Card>
                ))}
            </SimpleGrid>

            <Stack gap="md">
                <Title order={2}>Najnowsze projekty</Title>

                {isLoading && <Loader />}

                {isError && (
                    <Alert color="red" title="Błąd">
                        {error instanceof Error ? error.message : 'Nie udało się pobrać projektów.'}
                    </Alert>
                )}

                {!isLoading && !isError && latestProjects.length === 0 && (
                    <Text c="dimmed">Brak projektów na platformie.</Text>
                )}

                {latestProjects.map((project: ProjectModel) => (
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
                                to={`/discover/project/${project.id}`}
                            >
                                <ArrowRightIcon style={{ width: '70%', height: '70%' }} />
                            </ActionIcon>
                        </Group>
                    </Paper>
                ))}

                {data && data.length > LATEST_PROJECTS_COUNT && (
                    <Anchor component={Link} to="/discover" size="sm" fw={700}>
                        Zobacz wszystkie projekty
                    </Anchor>
                )}
            </Stack>
        </Stack>
    );
};
