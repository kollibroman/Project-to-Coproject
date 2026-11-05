import { Alert, Badge, Button, Group, Paper, Skeleton, Stack, Text, Title } from '@mantine/core';
import { CheckIcon, EnvelopeSimpleIcon, XIcon } from '@phosphor-icons/react';
import dayjs from 'dayjs';
import { useEffect, useRef, useState } from 'react';
import { useLocation, useNavigate } from 'react-router';
import type { ProjectInvitationModel } from '../../../client';
import { useAcceptInvitation, useDeclineInvitation, useMyInvitations } from '../../../hooks';
import { getProblemMessage } from '../../../Utils/problemMessage.ts';

/** Kotwica, do której prowadzi dzwonek w nagłówku. */
export const INVITATIONS_ANCHOR = 'invitations';

export const InvitationsSection = () => {
    const invitations = useMyInvitations();
    const accept = useAcceptInvitation();
    const decline = useDeclineInvitation();
    const navigate = useNavigate();

    const location = useLocation();
    const sectionRef = useRef<HTMLDivElement>(null);

    // Id zaproszenia, na które właśnie odpowiadamy, żeby pokazać loader tylko przy nim.
    const [answeringId, setAnsweringId] = useState<string | undefined>(undefined);

    // React Router nie przewija do kotwicy, więc robimy to po kliknięciu dzwonka.
    useEffect(() => {
        if (location.hash === `#${INVITATIONS_ANCHOR}`) {
            sectionRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }, [location.hash, location.key]);

    const handleAccept = (invitation: ProjectInvitationModel) => {
        setAnsweringId(invitation.id);
        accept.mutate(
            { path: { invitationId: invitation.id } },
            {
                onSuccess: () => navigate(`/project/${invitation.projectId}`),
                onSettled: () => setAnsweringId(undefined),
            },
        );
    };

    const handleDecline = (invitation: ProjectInvitationModel) => {
        setAnsweringId(invitation.id);
        decline.mutate(
            { path: { invitationId: invitation.id } },
            { onSettled: () => setAnsweringId(undefined) },
        );
    };

    const items = invitations.data ?? [];
    const answerError = accept.error ?? decline.error;

    return (
        <Paper withBorder p="md" id={INVITATIONS_ANCHOR} ref={sectionRef}>
            <Stack gap="md">
                <Group gap="xs">
                    <EnvelopeSimpleIcon size={24} />
                    <Title order={3}>Zaproszenia do projektów</Title>
                    {items.length > 0 && (
                        <Badge color="red" variant="filled" circle>
                            {items.length}
                        </Badge>
                    )}
                </Group>

                {invitations.isLoading && <Skeleton height={60} />}

                {invitations.isError && (
                    <Alert color="red" title="Błąd">
                        {getProblemMessage(invitations.error, 'Nie udało się pobrać zaproszeń.')}
                    </Alert>
                )}

                {answerError && (
                    <Alert color="red" title="Błąd">
                        {getProblemMessage(answerError, 'Nie udało się odpowiedzieć na zaproszenie.')}
                    </Alert>
                )}

                {!invitations.isLoading && !invitations.isError && items.length === 0 && (
                    <Text c="dimmed" size="sm">
                        Nie masz oczekujących zaproszeń.
                    </Text>
                )}

                {items.map((invitation) => {
                    const busy = answeringId === invitation.id;
                    return (
                        <Paper key={invitation.id} withBorder radius="sm" p="md">
                            <Group justify="space-between" align="flex-start" wrap="nowrap">
                                <Stack gap={4} style={{ minWidth: 0 }}>
                                    <Group gap="xs">
                                        <Text fw={600}>{invitation.projectName}</Text>
                                        <Badge variant="light">{invitation.roleName ?? 'Rola domyślna'}</Badge>
                                    </Group>
                                    <Text size="sm" c="dimmed">
                                        Zaprasza {invitation.invitedBy} ·{' '}
                                        {dayjs(invitation.createdAt).format('DD.MM.YYYY HH:mm')}
                                    </Text>
                                    {invitation.invitationContent && (
                                        <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>
                                            {invitation.invitationContent}
                                        </Text>
                                    )}
                                    {!invitation.invitationContent && invitation.projectDescription && (
                                        <Text size="sm" c="dimmed" lineClamp={2}>
                                            {invitation.projectDescription}
                                        </Text>
                                    )}
                                </Stack>

                                <Group gap="xs" wrap="nowrap">
                                    <Button
                                        variant="default"
                                        leftSection={<XIcon />}
                                        disabled={busy && accept.isPending}
                                        loading={busy && decline.isPending}
                                        onClick={() => handleDecline(invitation)}
                                    >
                                        Odrzuć
                                    </Button>
                                    <Button
                                        leftSection={<CheckIcon />}
                                        disabled={busy && decline.isPending}
                                        loading={busy && accept.isPending}
                                        onClick={() => handleAccept(invitation)}
                                    >
                                        Dołącz
                                    </Button>
                                </Group>
                            </Group>
                        </Paper>
                    );
                })}
            </Stack>
        </Paper>
    );
};
