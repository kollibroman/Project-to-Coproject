import { Stack, Text, Title } from '@mantine/core';

function App() {
  return (
    <Stack gap="sm">
      <Title order={1}>Pulpit</Title>
      <Text c="dimmed">
        Witaj w ProjectCowork. Wybierz sekcję z menu po lewej stronie.
      </Text>
    </Stack>
  );
}

export default App;
