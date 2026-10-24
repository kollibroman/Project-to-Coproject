import { Stack, Text, Title } from '@mantine/core';

export const SettingsPage = () => {
  return (
    <Stack gap="sm">
      <Title order={1}>Ustawienia</Title>
      <Text c="dimmed">Tu pojawią się ustawienia aplikacji.</Text>
    </Stack>
  );
};
