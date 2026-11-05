import {
    Group,
    Paper,
    SegmentedControl,
    Stack,
    Text,
    Title,
    useMantineColorScheme,
    type MantineColorScheme,
} from '@mantine/core';
import { useAuth } from '../../../hooks';
import { getEmailFromToken, getUserIdFromToken } from '../../../auth/claims.ts';
import { GithubConnectionSection } from '../Components/GithubConnectionSection.tsx';

const colorSchemeOptions: { label: string; value: MantineColorScheme }[] = [
    { label: 'Jasny', value: 'light' },
    { label: 'Ciemny', value: 'dark' },
    { label: 'Systemowy', value: 'auto' },
];

export const AccountPage = () => {
    const { token } = useAuth();
    const { colorScheme, setColorScheme } = useMantineColorScheme();

    const email = getEmailFromToken(token);
    const userId = getUserIdFromToken(token);

    return (
        <Stack gap="md">
            <Title order={1}>Konto</Title>

            <Paper withBorder p="md">
                <Stack gap="xs">
                    <Title order={3}>Profil</Title>
                    <Group gap="xs">
                        <Text c="dimmed" w={120}>E-mail</Text>
                        <Text>{email ?? '—'}</Text>
                    </Group>
                    <Group gap="xs">
                        <Text c="dimmed" w={120}>Id użytkownika</Text>
                        <Text ff="monospace" size="sm">{userId ?? '—'}</Text>
                    </Group>
                </Stack>
            </Paper>

            <Paper withBorder p="md">
                <Stack gap="xs">
                    <Title order={3}>Wygląd</Title>
                    <Text size="sm" c="dimmed">Motyw aplikacji</Text>
                    <SegmentedControl
                        w="fit-content"
                        value={colorScheme}
                        onChange={(value) => setColorScheme(value as MantineColorScheme)}
                        data={colorSchemeOptions}
                    />
                </Stack>
            </Paper>

            <GithubConnectionSection />
        </Stack>
    );
};
