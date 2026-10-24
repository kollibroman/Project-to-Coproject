import { Alert, Button, Group, Loader, Stack, Text, Title } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { Link } from 'react-router';
import { useProjects } from '../../../hooks';
import type { ProjectModel } from '../../../client';
import { ProjectUserPaper } from '../Components/ProjectUserPaper.tsx';
import { CreateProjectDialog } from '../Dialogs/CreateProjectDialog.tsx';

export const ProjectsPage = () => {
  const { data, isLoading, isError, error } = useProjects();
  const [opened, { open, close }] = useDisclosure(false);

  return (
    <Stack gap="md">
      <Group justify="space-between">
        <Title order={1}>Projekty</Title>
        <Group>
          <Button variant="default" component={Link} to="/settings">
            Ustawienia
          </Button>
          <Button onClick={open}>Nowy projekt</Button>
        </Group>
      </Group>

      <CreateProjectDialog opened={opened} onClose={close} />

      {isLoading && <Loader />}

      {isError && (
        <Alert color="red" title="Błąd">
          {error instanceof Error ? error.message : 'Nie udało się pobrać projektów.'}
        </Alert>
      )}

      {!isLoading && !isError && (
        <>
          {data && data.length > 0 ? (
            data.map((project: ProjectModel) => (
              <ProjectUserPaper key={project.id} project={project} />
            ))
          ) : (
            <Text c="dimmed">Brak projektów.</Text>
          )}
        </>
      )}
    </Stack>
  );
};
