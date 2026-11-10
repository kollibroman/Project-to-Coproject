import { Badge, Group, Stack, Text } from '@mantine/core';
import type { ReactNode } from 'react';

interface Props {
    /** Brak etykiety oznacza listę bez nagłówka (widok bez grupowania). */
    label?: string;
    color?: string;
    count: number;
    children: ReactNode;
}

export const TaskGroup = ({ label, color, count, children }: Props) => (
    <Stack gap="xs">
        {label && (
            <Group gap="xs">
                <Badge color={color} variant="dot" size="lg" radius="sm">
                    {label}
                </Badge>
                <Text size="sm" c="dimmed">
                    {count}
                </Text>
            </Group>
        )}
        {children}
    </Stack>
);
