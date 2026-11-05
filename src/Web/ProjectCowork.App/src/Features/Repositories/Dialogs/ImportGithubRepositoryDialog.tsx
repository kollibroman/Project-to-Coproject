import { Alert, Anchor, Button, Group, Loader, Modal, Select, Stack, Text, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { Link } from 'react-router';
import { useGithubRepositories, useGithubStatus, useImportGithubRepository } from '../../../hooks';
import { getProblemMessage } from '../../../Utils/problemMessage.ts';

interface Props {
    projectId: string;
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    cloneUrl: string;
    name: string;
}

const REPOSITORY_NAME_PATTERN = /^[A-Za-z0-9._-]+$/;

const toLocalName = (fullName: string) => fullName.split('/').pop() ?? fullName;

export const ImportGithubRepositoryDialog = ({ projectId, opened, onClose }: Props) => {
    const status = useGithubStatus();
    const isConnected = status.data?.isConnected ?? false;
    const repositories = useGithubRepositories(opened && isConnected);
    const importRepository = useImportGithubRepository();

    const form = useForm<FormValues>({
        initialValues: { cloneUrl: '', name: '' },
        validate: {
            cloneUrl: (value) => (value ? null : 'Wybierz repozytorium'),
            name: (value) => {
                const trimmed = value.trim();
                if (!trimmed) return 'Nazwa jest wymagana';
                return REPOSITORY_NAME_PATTERN.test(trimmed)
                    ? null
                    : 'Dozwolone są litery, cyfry oraz znaki . _ -';
            },
        },
    });

    const handleClose = () => {
        form.reset();
        importRepository.reset();
        onClose();
    };

    const handleRepositoryChange = (cloneUrl: string | null) => {
        form.setFieldValue('cloneUrl', cloneUrl ?? '');
        const selected = repositories.data?.find((repo) => repo.cloneUrl === cloneUrl);
        if (selected) {
            form.setFieldValue('name', toLocalName(selected.name));
        }
    };

    const handleSubmit = form.onSubmit((values) => {
        importRepository.mutate(
            { body: { projectId, cloneUrl: values.cloneUrl, repositoryName: values.name.trim() } },
            { onSuccess: handleClose },
        );
    });

    const renderContent = () => {
        if (status.isLoading) {
            return <Loader />;
        }

        if (status.isError) {
            return (
                <Alert color="red" title="Błąd">
                    {getProblemMessage(status.error, 'Nie udało się pobrać stanu połączenia z GitHubem.')}
                </Alert>
            );
        }

        if (!isConnected) {
            return (
                <Stack gap="md">
                    <Text>
                        Twoje konto nie jest połączone z GitHubem. Połącz je w{' '}
                        <Anchor component={Link} to="/account" onClick={handleClose}>
                            ustawieniach konta
                        </Anchor>
                        .
                    </Text>
                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Zamknij
                        </Button>
                    </Group>
                </Stack>
            );
        }

        return (
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {repositories.isError && (
                        <Alert color="red" title="Błąd">
                            {getProblemMessage(repositories.error, 'Nie udało się pobrać repozytoriów z GitHuba.')}
                        </Alert>
                    )}

                    {importRepository.isError && (
                        <Alert color="red" title="Błąd">
                            {getProblemMessage(importRepository.error, 'Nie udało się zaimportować repozytorium.')}
                        </Alert>
                    )}

                    <Select
                        label="Repozytorium GitHub"
                        placeholder={repositories.isLoading ? 'Ładowanie...' : 'Wybierz repozytorium'}
                        withAsterisk
                        searchable
                        nothingFoundMessage="Brak repozytoriów"
                        disabled={repositories.isLoading}
                        rightSection={repositories.isLoading ? <Loader size="xs" /> : undefined}
                        data={(repositories.data ?? []).map((repo) => ({
                            value: repo.cloneUrl,
                            label: repo.name,
                        }))}
                        value={form.values.cloneUrl || null}
                        onChange={handleRepositoryChange}
                        error={form.errors.cloneUrl}
                    />

                    <TextInput
                        label="Nazwa w projekcie"
                        placeholder="Nazwa repozytorium"
                        withAsterisk
                        {...form.getInputProps('name')}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit" loading={importRepository.isPending}>
                            Importuj
                        </Button>
                    </Group>
                </Stack>
            </form>
        );
    };

    return (
        <Modal opened={opened} onClose={handleClose} title="Import repozytorium z GitHuba" centered>
            {renderContent()}
        </Modal>
    );
};
