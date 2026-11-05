import { Alert, Anchor, Badge, Button, Group, Paper, PasswordInput, Skeleton, Stack, Text, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useDisclosure } from '@mantine/hooks';
import { GithubLogoIcon } from '@phosphor-icons/react';
import { useConnectGithub, useGithubStatus } from '../../../hooks';
import { getProblemMessage } from '../../../Utils/problemMessage.ts';

const NEW_TOKEN_URL = 'https://github.com/settings/personal-access-tokens/new';

interface FormValues {
    login: string;
    pat: string;
}

export const GithubConnectionSection = () => {
    const status = useGithubStatus();
    const connect = useConnectGithub();
    const [editing, { open: startEditing, close: stopEditing }] = useDisclosure(false);

    const form = useForm<FormValues>({
        initialValues: { login: '', pat: '' },
        validate: {
            login: (value) => (value.trim().length > 0 ? null : 'Login jest wymagany'),
            pat: (value) => (value.trim().length > 0 ? null : 'Token jest wymagany'),
        },
    });

    const isConnected = status.data?.isConnected ?? false;
    const showForm = !isConnected || editing;

    const handleCancel = () => {
        form.reset();
        connect.reset();
        stopEditing();
    };

    const handleSubmit = form.onSubmit((values) => {
        connect.mutate(
            { body: { login: values.login.trim(), pat: values.pat.trim() } },
            { onSuccess: handleCancel },
        );
    });

    return (
        <Paper withBorder p="md">
            <Stack gap="md">
                <Group justify="space-between">
                    <Group gap="xs">
                        <GithubLogoIcon size={24} />
                        <Title order={3}>GitHub</Title>
                    </Group>
                    {!status.isLoading && !status.isError && (
                        <Badge color={isConnected ? 'green' : 'gray'} variant="light">
                            {isConnected ? 'Połączono' : 'Niepołączono'}
                        </Badge>
                    )}
                </Group>

                {status.isLoading && <Skeleton height={36} />}

                {status.isError && (
                    <Alert color="red" title="Błąd">
                        {getProblemMessage(status.error, 'Nie udało się pobrać stanu połączenia z GitHubem.')}
                    </Alert>
                )}

                {isConnected && !editing && (
                    <Group justify="space-between">
                        <Text>
                            Połączono jako <b>{status.data?.githubUsername}</b>
                        </Text>
                        <Button variant="default" onClick={startEditing}>
                            Zmień token
                        </Button>
                    </Group>
                )}

                {!status.isLoading && !status.isError && showForm && (
                    <form onSubmit={handleSubmit}>
                        <Stack gap="sm">
                            <Text size="sm" c="dimmed">
                                Utwórz{' '}
                                <Anchor href={NEW_TOKEN_URL} target="_blank" rel="noreferrer">
                                    fine-grained personal access token
                                </Anchor>{' '}
                                z dostępem do repozytoriów (uprawnienie <i>Contents: Read-only</i>) i wklej go poniżej.
                            </Text>

                            {connect.isError && (
                                <Alert color="red" title="Błąd">
                                    {getProblemMessage(connect.error, 'Nie udało się połączyć konta GitHub.')}
                                </Alert>
                            )}

                            <TextInput
                                label="Login GitHub"
                                placeholder="np. octocat"
                                withAsterisk
                                autoComplete="username"
                                {...form.getInputProps('login')}
                            />
                            <PasswordInput
                                label="Personal access token"
                                placeholder="github_pat_..."
                                withAsterisk
                                autoComplete="off"
                                {...form.getInputProps('pat')}
                            />

                            <Group justify="flex-end">
                                {editing && (
                                    <Button variant="default" onClick={handleCancel}>
                                        Anuluj
                                    </Button>
                                )}
                                <Button type="submit" loading={connect.isPending} leftSection={<GithubLogoIcon />}>
                                    Połącz z GitHubem
                                </Button>
                            </Group>
                        </Stack>
                    </form>
                )}
            </Stack>
        </Paper>
    );
};
