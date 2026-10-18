import {
  AppShell,
  Burger,
  Button,
  Group,
  NavLink,
  ScrollArea,
  Text,
  Title,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { Outlet, useLocation, useNavigate } from 'react-router';
import { useAuth } from '../hooks';

type NavItem = {
  label: string;
  path: string;
};

const navItems: NavItem[] = [
  { label: 'Pulpit', path: '/' },
  { label: 'Projekty', path: '/projects' },
  { label: 'Ustawienia', path: '/settings' },
];

export function AppLayout() {
  const [mobileOpened, { toggle: toggleMobile, close: closeMobile }] =
    useDisclosure(false);
  const [desktopOpened, { toggle: toggleDesktop }] = useDisclosure(true);
  const navigate = useNavigate();
  const location = useLocation();
  const { logout } = useAuth();

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{
        width: 260,
        breakpoint: 'sm',
        collapsed: { mobile: !mobileOpened, desktop: !desktopOpened },
      }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" gap="sm" justify="space-between">
          <Group gap="sm">
            <Burger
              opened={mobileOpened}
              onClick={toggleMobile}
              hiddenFrom="sm"
              size="sm"
            />
            <Burger
              opened={desktopOpened}
              onClick={toggleDesktop}
              visibleFrom="sm"
              size="sm"
            />
            <Title order={3}>ProjectCowork</Title>
          </Group>
          <Button variant="subtle" size="sm" onClick={logout}>
            Wyloguj
          </Button>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="md">
        <AppShell.Section grow component={ScrollArea}>
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              label={item.label}
              active={
                item.path === '/'
                  ? location.pathname === '/'
                  : location.pathname.startsWith(item.path)
              }
              onClick={() => {
                navigate(item.path);
                closeMobile();
              }}
            />
          ))}
        </AppShell.Section>
        <AppShell.Section>
          <Text size="xs" c="dimmed">
            v0.0.0
          </Text>
        </AppShell.Section>
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
}
