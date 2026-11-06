import { Alert, Button, Group, Modal, Stack, Switch, Text, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useEffect, useMemo, useState } from 'react';
import type { PermissionDto } from '../../../../client';
import { PermissionMatrix } from '../../Components/PermissionMatrix.tsx';
import { ActionTypeValue, getPermissionId } from '../../permissionEnums.ts';
import type { ProjectRole, ProjectRoleInput } from '../projectAccessTypes.ts';

interface Props {
    role?: ProjectRole;
    catalog: PermissionDto[];
    opened: boolean;
    onClose: () => void;
    onSubmit: (input: ProjectRoleInput) => Promise<void>;
}

interface FormValues {
    name: string;
    isDefault: boolean;
}

export const ProjectRoleDialog = ({ role, catalog, opened, onClose, onSubmit }: Props) => {
    const isEdit = Boolean(role);

    const [granted, setGranted] = useState<Set<number>>(new Set());
    const [error, setError] = useState<string | null>(null);

    const form = useForm<FormValues>({
        initialValues: { name: '', isDefault: false },
        validate: {
            name: (value) => (value.trim().length > 0 ? null : 'Nazwa roli jest wymagana'),
        },
    });

    useEffect(() => {
        if (opened) {
            form.setValues({ name: role?.name ?? '', isDefault: role?.isDefault ?? false });
            setGranted(new Set(role?.permissionIds ?? []));
            setError(null);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [opened, role]);

    const allIds = useMemo(() => catalog.map(getPermissionId), [catalog]);
    const viewIds = useMemo(
        () => catalog.filter((x) => x.action === ActionTypeValue.View).map(getPermissionId),
        [catalog],
    );

    const handleToggle = (permission: PermissionDto, shouldGrant: boolean) => {
        const id = getPermissionId(permission);
        setGranted((prev) => {
            const next = new Set(prev);
            if (shouldGrant) {
                next.add(id);
            } else {
                next.delete(id);
            }
            return next;
        });
    };

    const handleClose = () => {
        form.reset();
        setError(null);
        onClose();
    };

    const handleSubmit = form.onSubmit(async (values) => {
        try {
            await onSubmit({ name: values.name, isDefault: values.isDefault, permissionIds: [...granted] });
            handleClose();
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Nie udało się zapisać roli.');
        }
    });

    // Zdjęcie flagi z roli domyślnej zostawiłoby projekt bez roli dla zaproszonych.
    const lockDefault = Boolean(role?.isDefault);

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={isEdit ? `Edytuj rolę „${role?.name}”` : 'Nowa rola'}
            size="xl"
            centered
        >
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {error && (
                        <Alert color="red" title="Błąd">
                            {error}
                        </Alert>
                    )}

                    <TextInput
                        label="Nazwa"
                        placeholder="np. Tester"
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('name')}
                    />

                    <Switch
                        label="Rola domyślna dla zaproszonych"
                        description={
                            lockDefault
                                ? 'Aby zmienić rolę domyślną, ustaw inną rolę jako domyślną.'
                                : 'Zaproszeni bez wybranej roli dostaną tę rolę.'
                        }
                        disabled={lockDefault}
                        {...form.getInputProps('isDefault', { type: 'checkbox' })}
                    />

                    <Group justify="space-between">
                        <Text size="sm" c="dimmed">
                            Zaznaczono {granted.size} z {catalog.length} uprawnień
                        </Text>
                        <Group gap="xs">
                            <Button size="xs" variant="default" onClick={() => setGranted(new Set(allIds))}>
                                Wszystko
                            </Button>
                            <Button size="xs" variant="default" onClick={() => setGranted(new Set(viewIds))}>
                                Tylko odczyt
                            </Button>
                            <Button size="xs" variant="default" onClick={() => setGranted(new Set())}>
                                Wyczyść
                            </Button>
                        </Group>
                    </Group>

                    <PermissionMatrix permissions={catalog} granted={granted} onToggle={handleToggle} />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit">{isEdit ? 'Zapisz' : 'Utwórz'}</Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
