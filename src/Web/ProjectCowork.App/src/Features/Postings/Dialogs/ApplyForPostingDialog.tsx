import { Alert, Button, FileInput, Group, Modal, Stack, Textarea } from '@mantine/core';
import { useForm } from '@mantine/form';
import { PaperclipIcon } from '@phosphor-icons/react';
import { useApplyForPosting } from '../../../hooks';
import type { ProjectPostingDto } from '../../../client';

interface Props {
    posting?: ProjectPostingDto;
    opened: boolean;
    onClose: () => void;
}

interface FormValues {
    description: string;
    attachments: File[];
}

export const ApplyForPostingDialog = ({ posting, opened, onClose }: Props) => {
    const apply = useApplyForPosting();

    const form = useForm<FormValues>({
        initialValues: {
            description: '',
            attachments: [],
        },
        validate: {
            description: (value) =>
                value.trim().length > 0 ? null : 'Opis zgłoszenia jest wymagany',
        },
    });

    const handleClose = () => {
        form.reset();
        apply.reset();
        onClose();
    };

    const handleSubmit = form.onSubmit((values) => {
        if (!posting) {
            return;
        }

        apply.mutate(
            {
                body: {
                    projectPostingId: posting.id,
                    description: values.description.trim(),
                    attachments: values.attachments,
                },
            },
            { onSuccess: () => handleClose() },
        );
    });

    return (
        <Modal opened={opened} onClose={handleClose} title="Aplikuj na ofertę" centered>
            <form onSubmit={handleSubmit}>
                <Stack gap="md">
                    {apply.isError && (
                        <Alert color="red" title="Błąd">
                            {apply.error instanceof Error
                                ? apply.error.message
                                : 'Nie udało się wysłać zgłoszenia.'}
                        </Alert>
                    )}

                    <Textarea
                        label="Opis zgłoszenia"
                        placeholder="Napisz kilka słów o sobie i dlaczego pasujesz do projektu"
                        withAsterisk
                        autosize
                        minRows={4}
                        data-autofocus
                        {...form.getInputProps('description')}
                    />

                    <FileInput
                        label="Załączniki"
                        placeholder="Dodaj pliki (opcjonalnie)"
                        leftSection={<PaperclipIcon />}
                        multiple
                        clearable
                        value={form.values.attachments}
                        onChange={(files) => form.setFieldValue('attachments', files)}
                    />

                    <Group justify="flex-end">
                        <Button variant="default" onClick={handleClose}>
                            Anuluj
                        </Button>
                        <Button type="submit" loading={apply.isPending}>
                            Wyślij zgłoszenie
                        </Button>
                    </Group>
                </Stack>
            </form>
        </Modal>
    );
};
