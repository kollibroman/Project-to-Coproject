import { Alert, Button, Group, Modal, Select, Stack, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useEffect, useState } from 'react';
import type { ProjectRole } from '../projectAccessTypes.ts';

interface Props {
    roles: ProjectRole[];
    opened: boolean;
    onClose: () => void;
    /** Rzuca Error z komunikatem, gdy zaproszenie się nie uda. */
    onSubmit: (email: string, roleId: string | null) => void;
}

interface FormValues {
    email: string;
    roleId: string | null;
}

export const InviteMemberDialog = ({ roles, opened, onClose, onSubmit }: Props) => {
    const [error, setError] = useState<string | null>(null);

    const form = useForm<FormValues>({
        initialValues: { email: '', roleId: null },
        validate: {
            email: (value) => (/^\S+@\S+\.\S+$/.test(value.trim()) ? null : 'Podaj poprawny adres e-mail'),
        },
    });

    useEffect(() => {
        if (opened) {
            form.setValues({ email: '', roleId: roles.find((r) => r.isDefault)?.id ?? null });
            setError(null);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [opened]);

    const handleClose = () => {
        form.reset();
        setError(null);
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        try {
            onSubmit(values.email, values.roleId);
            handleClose();
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Nie udało się zaprosić użytkownika.');
        }
    });

    return (
        <Modal opened={opened} onClose={handleClose} title="Zaproś do projektu" centered>
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {error && (
                        <Alert color="red" title="Błąd">
                            {error}
                        </Alert>
                    )}

                    <TextInput
                        label="E-mail"
                        placeholder="osoba@example.com"
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('email')}
                    />

                    <Select
                        label="Rola"
                        description="Bez wyboru użytkownik dostanie rolę domyślną."
                        data={roles.map((r) => ({
                            value: r.id,
                            label: r.isDefault ? `${r.name} (domyślna)` : r.name,
                        }))}
                        clearable
                        {...form.getInputProps('roleId')}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit">Zaproś</Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
