import { Anchor, Badge, Group, Paper, Stack, Text, ThemeIcon, Title } from '@mantine/core';
import { EnvelopeSimpleIcon, PhoneIcon } from '@phosphor-icons/react';
import type { ComponentType, ReactNode } from 'react';
import type { ProjectModel } from '../../../client';

interface Props {
    project: ProjectModel;
}

interface ContactRowProps {
    icon: ComponentType<{ size?: number }>;
    label: string;
    children: ReactNode;
}

const ContactRow = ({ icon: Icon, label, children }: ContactRowProps) => (
    <Group gap="md" wrap="nowrap">
        <ThemeIcon variant="light" size={40} radius="md">
            <Icon size={22} />
        </ThemeIcon>
        <Stack gap={0}>
            <Text size="xs" c="dimmed">
                {label}
            </Text>
            {children}
        </Stack>
    </Group>
);

const NotProvided = () => <Text c="dimmed">Nie podano</Text>;

/** Widok projektu dla konta firmy: tylko szczegóły i dane kontaktowe, bez ogłoszeń. */
export const BusinessProjectPreview = ({ project }: Props) => (
    <Stack gap="lg">
        <Stack gap="xs">
            <Title order={1}>{project.name}</Title>
            <Badge w="fit-content" variant="light">
                Podgląd projektu
            </Badge>
        </Stack>

        <Paper withBorder shadow="md" radius="sm" p="xl">
            <Stack gap="xs">
                <Title order={3}>Opis projektu</Title>
                <Text size="sm" c="dimmed">
                    {project.description}
                </Text>
            </Stack>
        </Paper>

        <Paper withBorder shadow="md" radius="sm" p="xl">
            <Stack gap="md">
                <Title order={3}>Dane kontaktowe</Title>
                <ContactRow icon={EnvelopeSimpleIcon} label="E-mail">
                    {project.contactEmail ? (
                        <Anchor href={`mailto:${project.contactEmail}`}>{project.contactEmail}</Anchor>
                    ) : (
                        <NotProvided />
                    )}
                </ContactRow>
                <ContactRow icon={PhoneIcon} label="Telefon">
                    {project.contactPhone ? (
                        <Anchor href={`tel:${project.contactPhone}`}>{project.contactPhone}</Anchor>
                    ) : (
                        <NotProvided />
                    )}
                </ContactRow>
            </Stack>
        </Paper>
    </Stack>
);
