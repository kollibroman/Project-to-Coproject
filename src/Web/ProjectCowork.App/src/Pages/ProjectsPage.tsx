import { Alert, Loader, Stack, Table, Text, Title } from '@mantine/core';
import { useProjects } from '../hooks';

export const ProjectsPage = () => {
  const { data, isLoading, isError, error } = useProjects();

  return (
    <Stack gap="md">
      <Title order={1}>Projekty</Title>

      {isLoading && <Loader />}

      {isError && (
        <Alert color="red" title="Błąd">
          {error instanceof Error ? error.message : 'Nie udało się pobrać projektów.'}
        </Alert>
      )}

      {!isLoading && !isError && (
        <>
          {data && data.length > 0 ? (
            <Table striped highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Nazwa</Table.Th>
                  <Table.Th>Opis</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {data.map((project) => (
                  <Table.Tr key={project.id}>
                    <Table.Td>{project.name}</Table.Td>
                    <Table.Td>{project.description}</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          ) : (
            <Text c="dimmed">Brak projektów.</Text>
          )}
        </>
      )}
    </Stack>
  );
};
