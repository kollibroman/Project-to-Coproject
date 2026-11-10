import { Alert, Button, Group, Modal, Select, Stack } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useEffect } from 'react';
import { useCreatePermission, useUpdatePermission } from '../../../hooks';
import type { PermissionDto } from '../../../client';
import { actionOptions, getPermissionId, resourceOptions, toSelectData } from '../permissionEnums.ts';
import { getErrorMessage } from '../getErrorMessage.ts';

interface Props {
    permission?: PermissionDto;
    /** Istniejące uprawnienia, żeby wykryć duplikat przed wysłaniem. */
    existing: PermissionDto[];
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    resource: string | null;
    action: string | null;
}

export const PermissionDialog = ({ permission, existing, opened, onClose }: Props) => {
    const isEdit = Boolean(permission);

    const createPermission = useCreatePermission();
    const updatePermission = useUpdatePermission();
    const mutation = isEdit ? updatePermission : createPermission;

    const form = useForm<FormValues>({
        initialValues: {
            resource: null,
            action: null,
        },
        validate: {
            resource: (value) => (value !== null ? null : 'Zasób jest wymagany'),
            action: (value, values) => {
                if (value === null) {
                    return 'Akcja jest wymagana';
                }
                const duplicate = existing.some(
                    (x) =>
                        x.resource === Number(values.resource) &&
                        x.action === Number(value) &&
                        (!permission || getPermissionId(x) !== getPermissionId(permission)),
                );
                return duplicate ? 'Takie uprawnienie już istnieje' : null;
            },
        },
    });

    useEffect(() => {
        if (opened) {
            form.setValues({
                resource: permission ? String(permission.resource) : null,
                action: permission ? String(permission.action) : null,
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [opened, permission]);

    const handleClose = () => {
        form.reset();
        createPermission.reset();
        updatePermission.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        const body = {
            resource: Number(values.resource),
            action: Number(values.action),
        };

        if (isEdit && permission) {
            updatePermission.mutate(
                { path: { permissionId: getPermissionId(permission) }, body },
                { onSuccess: () => handleClose() },
            );
        } else {
            createPermission.mutate({ body }, { onSuccess: () => handleClose() });
        }
    });

    return (
        <Modal
            opened={opened}
            onClose={handleClose}
            title={isEdit ? 'Edytuj uprawnienie' : 'Nowe uprawnienie'}
            centered
        >
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {mutation.isError && (
                        <Alert color="red" title="Błąd">
                            {getErrorMessage(
                                mutation.error,
                                'Nie udało się zapisać uprawnienia.',
                                isEdit ? 'Permission.Permissions.Update' : 'Permission.Permissions.Create',
                            )}
                        </Alert>
                    )}

                    <Select
                        label="Zasób"
                        placeholder="Wybierz zasób"
                        data={toSelectData(resourceOptions)}
                        allowDeselect={false}
                        withAsterisk
                        data-autofocus
                        {...form.getInputProps('resource')}
                    />

                    <Select
                        label="Akcja"
                        placeholder="Wybierz akcję"
                        data={toSelectData(actionOptions)}
                        allowDeselect={false}
                        withAsterisk
                        {...form.getInputProps('action')}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit" loading={mutation.isPending}>
                            {isEdit ? 'Zapisz' : 'Utwórz'}
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
