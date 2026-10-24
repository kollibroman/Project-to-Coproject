import { Alert, Button, Group, Loader, Paper, Stack, Text, Title } from '@mantine/core';
import { InfoIcon, UserPlusIcon } from '@phosphor-icons/react';
import { useDisclosure } from '@mantine/hooks';
import { useEffect, useMemo, useState } from 'react';
import { usePostings, useGetPostingApplications } from '../../../hooks';
import type { ProjectApplicationDto } from '../../../client';
import { InviteUserDialog } from '../Dialogs/InviteUserDialog.tsx';

type ProjectUsersTabProps = {
    projectId: string | undefined;
};

type ApplicantRow = ProjectApplicationDto & { postingId: string };

export const ProjectUsersTab = ({ projectId }: ProjectUsersTabProps) => {
    const { data: postings, isLoading: postingsLoading } = usePostings(projectId ?? '');
    const { mutateAsync, reset } = useGetPostingApplications();

    const [inviteOpened, { open: openInvite, close: closeInvite }] = useDisclosure(false);

    const [applicants, setApplicants] = useState<ApplicantRow[] | undefined>(undefined);

    const postingIdsKey = useMemo(
        () => (postings ?? []).map((p) => p.id).join(','),
        [postings],
    );

    // Zgłoszenia nie mają zbiorczego endpointu w API, więc pobieramy je
    // osobno dla każdego ogłoszenia i scalamy w jedną listę.
    useEffect(() => {
        const ids = postingIdsKey ? postingIdsKey.split(',') : [];

        if (ids.length === 0) {
            setApplicants([]);
            return;
        }

        let cancelled = false;
        setApplicants(undefined);

        void Promise.all(
            ids.map(async (projectPostingId) => {
                try {
                    const result = await mutateAsync({ path: { projectPostingId } });
                    return (result ?? []).map<ApplicantRow>((a) => ({
                        ...a,
                        postingId: projectPostingId,
                    }));
                } catch {
                    return [] as ApplicantRow[];
                }
            }),
        ).then((all) => {
            if (!cancelled) {
                setApplicants(all.flat());
            }
        });

        return () => {
            cancelled = true;
            reset();
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [postingIdsKey]);

    const uniqueUsers = useMemo(() => {
        const seen = new Map<string, ApplicantRow>();
        for (const row of applicants ?? []) {
            if (!seen.has(row.userId)) {
                seen.set(row.userId, row);
            }
        }
        return Array.from(seen.values());
    }, [applicants]);

    const isLoading = postingsLoading || applicants === undefined;

    return (
        <Stack gap="md">
            <Group justify="flex-end">
                <Button
                    leftSection={<UserPlusIcon />}
                    onClick={openInvite}
                    disabled={!projectId}
                >
                    Zaproś użytkownika
                </Button>
            </Group>

            <Alert color="blue" title="Informacja" icon={<InfoIcon />}>
                Lista poniżej pokazuje osoby, które zgłosiły się do ogłoszeń tego
                projektu. Zaproszeni współpracownicy są dodawani do projektu, ale API
                nie udostępnia jeszcze endpointu listującego członków.
            </Alert>

            {isLoading && <Loader />}

            {!isLoading && uniqueUsers.length === 0 && (
                <Text c="dimmed">Brak użytkowników powiązanych z projektem.</Text>
            )}

            {!isLoading &&
                uniqueUsers.map((user) => (
                    <Paper key={user.userId} withBorder shadow="md" radius="sm" p="xl">
                        <Group justify="space-between" align="flex-start" wrap="nowrap">
                            <Stack gap={4}>
                                <Title order={4}>{user.userId}</Title>
                                <Text size="sm" c="dimmed">
                                    {user.description}
                                </Text>
                            </Stack>
                            <Text size="sm" c="dimmed">
                                Kandydat
                            </Text>
                        </Group>
                    </Paper>
                ))}

            {projectId && (
                <InviteUserDialog
                    projectId={projectId}
                    opened={inviteOpened}
                    onClose={closeInvite}
                />
            )}
        </Stack>
    );
};
