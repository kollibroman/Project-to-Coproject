import { Stack, Text, Title } from '@mantine/core';
import { AppLayout } from './Layout/AppLayout.tsx';

function App() {
  return (
    <AppLayout>
      <Stack gap="sm">
          // TODO: Landing page
        <Title order={1}>Pulpit</Title>
        <Text c="dimmed">
          Witaj w ProjectCowork. Wybierz sekcję z menu po lewej stronie.
        </Text>
      </Stack>
    </AppLayout>
  );
}

export default App;
