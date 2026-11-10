import {useRepositories} from "../../../hooks";
import {Button, Group, Skeleton, Stack, Text} from "@mantine/core";
import {useDisclosure} from "@mantine/hooks";
import {GithubLogoIcon, PlusIcon} from "@phosphor-icons/react";
import {RepositoryPaper} from "../../Repositories/Components/RepositoryPaper.tsx";
import {CreateRepositoryDialog} from "../../Repositories/Dialogs/CreateRepositoryDialog.tsx";
import {ImportGithubRepositoryDialog} from "../../Repositories/Dialogs/ImportGithubRepositoryDialog.tsx";

type RepositoryTabProps = {
    projectId: string | undefined;
};

export const RepositoryTab = ({ projectId }: RepositoryTabProps) => {

    const { data, isLoading } = useRepositories(projectId);
    const [opened, { open, close }] = useDisclosure(false);
    const [importOpened, { open: openImport, close: closeImport }] = useDisclosure(false);

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
                        variant="default"
                        leftSection={<GithubLogoIcon />}
                        onClick={openImport}
                        disabled={!projectId}
                    >
                        Importuj z GitHuba
                    </Button>
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
                <>
                    <CreateRepositoryDialog projectId={projectId} opened={opened} onClose={close} />
                    <ImportGithubRepositoryDialog projectId={projectId} opened={importOpened} onClose={closeImport} />
                </>
            )}
        </>
    );
}
