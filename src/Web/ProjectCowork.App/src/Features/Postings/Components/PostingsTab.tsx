import { Alert, Button, Group, Loader, SegmentedControl, Stack, Text } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { PlusIcon } from '@phosphor-icons/react';
import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { usePostings, useUpdatePosting } from '../../../hooks';
import { getApiPostingsByProjectIdQueryKey } from '../../../client/@tanstack/react-query.gen';
import type { ProjectPostingDto } from '../../../client';
import { PostingPaper } from './PostingPaper.tsx';
import { PostingDialog } from '../Dialogs/PostingDialog.tsx';
import { PostingApplicationsDialog } from '../Dialogs/PostingApplicationsDialog.tsx';

type PostingsTabProps = {
    projectId: string | undefined;
};

type ActivityFilter = 'all' | 'active' | 'inactive';

export const PostingsTab = ({ projectId }: PostingsTabProps) => {
    const [filter, setFilter] = useState<ActivityFilter>('all');

    const isActiveFilter =
        filter === 'all' ? undefined : filter === 'active';

    const { data, isLoading, isError, error } = usePostings(
        projectId ?? '',
        isActiveFilter,
    );
    const queryClient = useQueryClient();
    const toggle = useUpdatePosting();

    const [editorOpened, { open: openEditor, close: closeEditor }] = useDisclosure(false);
    const [applicationsOpened, { open: openApplications, close: closeApplications }] =
        useDisclosure(false);

    const [selected, setSelected] = useState<ProjectPostingDto | undefined>(undefined);
    const [togglingId, setTogglingId] = useState<string | undefined>(undefined);

    const handleCreate = () => {
        setSelected(undefined);
        openEditor();
    };

    const handleEdit = (posting: ProjectPostingDto) => {
        setSelected(posting);
        openEditor();
    };

    const handleShowApplications = (posting: ProjectPostingDto) => {
        setSelected(posting);
        openApplications();
    };

    const invalidateList = () => {
        if (!projectId) {
            return;
        }
        // Unieważniamy wszystkie warianty listy (różne filtry isActive) dla tego projektu.
        const id = getApiPostingsByProjectIdQueryKey({ path: { projectId } })[0]._id;
        void queryClient.invalidateQueries({
            predicate: (query) => {
                const key = query.queryKey[0] as { _id?: string; path?: { projectId?: string } };
                return key?._id === id && key?.path?.projectId === projectId;
            },
        });
    };

    const handleToggleActive = (posting: ProjectPostingDto) => {
        setTogglingId(posting.id);
        toggle.mutate(
            {
                body: {
                    projectPostingId: posting.id,
                    jobDescription: posting.jobDescription,
                    projectDescription: posting.projectDescription,
                    isActive: !posting.isActive,
                },
            },
            {
                onSuccess: () => invalidateList(),
                onSettled: () => setTogglingId(undefined),
            },
        );
    };

    return (
        <>
            <Stack gap="md">
                <Group justify="space-between">
                    <SegmentedControl
                        value={filter}
                        onChange={(value) => setFilter(value as ActivityFilter)}
                        data={[
                            { label: 'Wszystkie', value: 'all' },
                            { label: 'Aktywne', value: 'active' },
                            { label: 'Nieaktywne', value: 'inactive' },
                        ]}
                    />
                    <Button
                        leftSection={<PlusIcon />}
                        onClick={handleCreate}
                        disabled={!projectId}
                    >
                        Dodaj ogłoszenie
                    </Button>
                </Group>

                {isError && (
                    <Alert color="red" title="Błąd">
                        {error instanceof Error
                            ? error.message
                            : 'Nie udało się pobrać ogłoszeń.'}
                    </Alert>
                )}

                {isLoading && <Loader />}

                {!isLoading && !isError && (!data || data.length === 0) && (
                    <Text c="dimmed">Brak ogłoszeń.</Text>
                )}

                {!isLoading &&
                    !isError &&
                    data?.map((posting) => (
                        <PostingPaper
                            key={posting.id}
                            posting={posting}
                            onEdit={handleEdit}
                            onToggleActive={handleToggleActive}
                            onShowApplications={handleShowApplications}
                            toggling={togglingId === posting.id}
                        />
                    ))}
            </Stack>

            {projectId && (
                <PostingDialog
                    projectId={projectId}
                    posting={selected}
                    opened={editorOpened}
                    onClose={closeEditor}
                />
            )}

            <PostingApplicationsDialog
                posting={selected}
                opened={applicationsOpened}
                onClose={closeApplications}
            />
        </>
    );
};
