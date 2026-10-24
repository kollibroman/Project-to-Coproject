import {useRepositories} from "../../../hooks";
import {Button, Group, Skeleton, Stack, Text} from "@mantine/core";
import {useDisclosure} from "@mantine/hooks";
import {PlusIcon} from "@phosphor-icons/react";
import {RepositoryPaper} from "../../Repositories/Components/RepositoryPaper.tsx";
import {CreateRepositoryDialog} from "../../Repositories/Dialogs/CreateRepositoryDialog.tsx";

type RepositoryTabProps = {
    projectId: string | undefined;
};

export const RepositoryTab = ({ projectId }: RepositoryTabProps) => {

    const { data, isLoading } = useRepositories(projectId);
    const [opened, { open, close }] = useDisclosure(false);

    if(isLoading)
    {
        return (
            <>
                <Skeleton visible={isLoading}>
                    Ładowanie repozytoriów....
                </Skeleton>
            </>
        )
    }

    return (
        <>
            <Stack gap="md">
                <Group justify="flex-end">
                    <Button
                        leftSection={<PlusIcon />}
                        onClick={open}
                        disabled={!projectId}
                    >
                        Dodaj repozytorium
                    </Button>
                </Group>

                {!data || data.length === 0 ? (
                    <Text c="dimmed">Brak repozytoriów.</Text>
                ) : (
                    data.map(rep => (<RepositoryPaper repository={rep} key={rep.id} />))
                )}
            </Stack>

            {projectId && (
                <CreateRepositoryDialog projectId={projectId} opened={opened} onClose={close} />
            )}
        </>
    );
}
