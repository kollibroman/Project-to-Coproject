import { Alert, Badge, Button, Group, Loader, Paper, Stack, Text, Title } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useMemo, useState } from 'react';
import { Navigate, useParams } from 'react-router';
import { useDiscoverProjects, usePostings } from '../../../hooks';
import type { ProjectPostingDto } from '../../../client';
import { ApplyForPostingDialog } from '../../Postings/Dialogs/ApplyForPostingDialog.tsx';

export const ProjectPreviewPage = () => {
    const { projectId } = useParams<{ projectId: string }>();
    const projectsQuery = useDiscoverProjects();
    const postingsQuery = usePostings(projectId ?? '', true);
    const [applicationOpened, { open: openApplication, close: closeApplication }] = useDisclosure(false);
    const [selectedPosting, setSelectedPosting] = useState<ProjectPostingDto | undefined>(undefined);

    const project = useMemo(
        () => projectsQuery.data?.find((item) => item.id === projectId),
        [projectsQuery.data, projectId],
    );

    if (!projectId) {
        return <Navigate to="/discover" replace />;
    }

    if (projectsQuery.isLoading || postingsQuery.isLoading) {
        return <Loader />;
    }

    if (projectsQuery.isError || postingsQuery.isError || !project) {
        return (
            <Alert color="red" title="Nie udało się otworzyć projektu">
                Projekt nie jest dostępny w widoku odkrywania albo nie ma już aktywnych ogłoszeń.
            </Alert>
        );
    }

    return (
        <Stack gap="lg">
            <Stack gap="xs">
                <Title order={1}>{project.name}</Title>
                <Text c="dimmed">{project.description}</Text>
                <Badge w="fit-content" variant="light">
                    Podgląd projektu
                </Badge>
            </Stack>

            <Stack gap="md">
                <Title order={2}>Aktywne ogłoszenia</Title>
                {postingsQuery.data?.length === 0 && (
                    <Text c="dimmed">Ten projekt nie ma obecnie aktywnych ogłoszeń.</Text>
                )}
                {postingsQuery.data?.map((posting: ProjectPostingDto) => (
                    <Paper key={posting.id} withBorder p="lg">
                        <Group justify="space-between" align="flex-start" wrap="nowrap">
                            <Stack gap="xs">
                                <Title order={3}>Oferta współpracy</Title>
                                <Text>{posting.jobDescription}</Text>
                                <Text size="sm" c="dimmed">
                                    {posting.projectDescription}
                                </Text>
                            </Stack>
                            <Button
                                onClick={() => {
                                    setSelectedPosting(posting);
                                    openApplication();
                                }}
                            >
                                Aplikuj
                            </Button>
                        </Group>
                    </Paper>
                ))}
            </Stack>

            <ApplyForPostingDialog
                posting={selectedPosting}
                opened={applicationOpened}
                onClose={closeApplication}
            />
        </Stack>
    );
};
