import { Card, SimpleGrid, Stack, Text, ThemeIcon, Title } from '@mantine/core';
import { BriefcaseIcon, CompassIcon, FolderIcon } from '@phosphor-icons/react';
import { Link } from 'react-router';
import type { ComponentType } from 'react';

type LandingTile = {
  title: string;
  description: string;
  to: string;
  icon: ComponentType<{ size?: number }>;
  color: string;
};

const tiles: LandingTile[] = [
  {
    title: 'Twoje projekty',
    description: 'Projekty, do których należysz lub którymi zarządzasz.',
    to: '/projects',
    icon: FolderIcon,
    color: 'blue',
  },
  {
    title: 'Odkryj',
    description:
      'Przeglądaj projekty na platformie, do których nie należysz, i zobacz ich oferty.',
    to: '/discover',
    icon: CompassIcon,
    color: 'teal',
  },
  {
    title: 'Oferty',
    description: 'Wszystkie otwarte oferty, na które możesz aplikować.',
    to: '/postings',
    icon: BriefcaseIcon,
    color: 'grape',
  },
];

function App() {
  return (
    <Stack gap="lg">
      <Stack gap="xs">
        <Title order={1}>Pulpit</Title>
      </Stack>
      <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="md">
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
    </Stack>
  );
}

export default App;
