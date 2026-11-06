import {
    ActionIcon,
    Button,
    CopyButton,
    Group,
    Popover,
    Select,
    Skeleton,
    Stack,
    Tabs,
    Text,
    TextInput,
    Title,
    Tooltip,
} from "@mantine/core";
import { CheckIcon, CodeIcon, CopyIcon, GitBranchIcon, GitCommitIcon } from "@phosphor-icons/react";
import { Navigate, useParams, useSearchParams } from "react-router";
import { useRepository, useRepositoryBranches } from "../../../hooks";
import { RepositoryFileBrowser } from "../Components/RepositoryFileBrowser";
import { RepositoryCommitList } from "../Components/RepositoryCommitList";

type RepositoryTab = "code" | "commits";

export const RepositoryPage = () => {
    const { repositoryId } = useParams<{ repositoryId: string }>();
    const [searchParams, setSearchParams] = useSearchParams();

    const { data, isLoading, isError } = useRepository(repositoryId ?? "");
    const branchesQuery = useRepositoryBranches(repositoryId ?? "");

    // Stan przeglądarki trzymany w URL: ?tab=code&branch=main&path=src&file=src/Program.cs
    const tab: RepositoryTab = searchParams.get("tab") === "commits" ? "commits" : "code";
    const defaultBranch =
        branchesQuery.data?.find((branch) => branch.isDefault)?.name ?? branchesQuery.data?.[0]?.name ?? "";
    const branch = searchParams.get("branch") ?? defaultBranch;
    const path = searchParams.get("path") ?? "";
    const filePath = searchParams.get("file");

    const updateParams = (changes: Record<string, string | null>) => {
        setSearchParams((previous) => {
            const next = new URLSearchParams(previous);
            Object.entries(changes).forEach(([key, value]) => {
                if (value === null || value === "") next.delete(key);
                else next.set(key, value);
            });
            return next;
        });
    };

    if (isError) {
        return <Navigate to={"/NotFound"} replace={true} />;
    }

    if (isLoading) {
        return (
            <Skeleton visible={isLoading}>
                Ładowanie repozytorium....
            </Skeleton>
        );
    }

    return (
        <Stack gap="md">
            <Group justify="space-between" align="center">
                <Title order={1}>{data?.name}</Title>

                <Popover width={360} position="bottom-end" withArrow shadow="md">
                    <Popover.Target>
                        <Button color="green" leftSection={<CodeIcon />}>
                            Code
                        </Button>
                    </Popover.Target>
                    <Popover.Dropdown>
                        <Stack gap="xs">
                            <Text fw={600} size="sm">
                                Klonuj
                            </Text>
                            <Group gap="xs" wrap="nowrap">
                                <TextInput
                                    readOnly
                                    value={data?.repoUrl ?? ""}
                                    styles={{ root: { flex: 1 } }}
                                    onFocus={(event) => event.currentTarget.select()}
                                />
                                <CopyButton value={data?.repoUrl ?? ""} timeout={2000}>
                                    {({ copied, copy }) => (
                                        <Tooltip
                                            label={copied ? "Skopiowano" : "Kopiuj"}
                                            withArrow
                                        >
                                            <ActionIcon
                                                color={copied ? "teal" : "gray"}
                                                variant="subtle"
                                                size="lg"
                                                onClick={copy}
                                            >
                                                {copied ? (
                                                    <CheckIcon style={{ width: "70%", height: "70%" }} />
                                                ) : (
                                                    <CopyIcon style={{ width: "70%", height: "70%" }} />
                                                )}
                                            </ActionIcon>
                                        </Tooltip>
                                    )}
                                </CopyButton>
                            </Group>
                        </Stack>
                    </Popover.Dropdown>
                </Popover>
            </Group>

            <Tabs
                value={tab}
                onChange={(value) => updateParams({ tab: value === "commits" ? "commits" : null })}
                keepMounted={false}
            >
                <Group justify="space-between" align="flex-end" mb="md">
                    <Tabs.List>
                        <Tabs.Tab value="code" leftSection={<CodeIcon />}>
                            Kod
                        </Tabs.Tab>
                        <Tabs.Tab value="commits" leftSection={<GitCommitIcon />}>
                            Commity
                        </Tabs.Tab>
                    </Tabs.List>

                    <Select
                        w={220}
                        leftSection={<GitBranchIcon />}
                        placeholder={branchesQuery.isLoading ? "Ładowanie gałęzi..." : "Brak gałęzi"}
                        data={branchesQuery.data?.map((b) => b.name) ?? []}
                        value={branch || null}
                        onChange={(value) => updateParams({ branch: value, path: null, file: null })}
                        allowDeselect={false}
                        disabled={!branchesQuery.data?.length}
                    />
                </Group>

                <Tabs.Panel value="code">
                    {branch ? (
                        <RepositoryFileBrowser
                            repositoryId={repositoryId ?? ""}
                            repositoryName={data?.name ?? ""}
                            branch={branch}
                            path={path}
                            filePath={filePath}
                            onOpenFolder={(folder) => updateParams({ path: folder, file: null })}
                            onOpenFile={(file) => updateParams({ file })}
                        />
                    ) : (
                        <EmptyRepository repoUrl={data?.repoUrl ?? ""} isLoading={branchesQuery.isLoading} />
                    )}
                </Tabs.Panel>

                <Tabs.Panel value="commits">
                    {branch ? (
                        <RepositoryCommitList repositoryId={repositoryId ?? ""} branch={branch} />
                    ) : (
                        <EmptyRepository repoUrl={data?.repoUrl ?? ""} isLoading={branchesQuery.isLoading} />
                    )}
                </Tabs.Panel>
            </Tabs>
        </Stack>
    );
};

const EmptyRepository = ({ repoUrl, isLoading }: { repoUrl: string; isLoading: boolean }) => {
    if (isLoading) {
        return <Skeleton height={160} />;
    }

    return (
        <Stack gap="xs">
            <Text fw={600}>To repozytorium jest puste</Text>
            <Text size="sm" c="dimmed">
                Wypchnij istniejące repozytorium z wiersza poleceń:
            </Text>
            <TextInput
                readOnly
                ff="monospace"
                value={`git remote add origin ${repoUrl} && git push -u origin main`}
                onFocus={(event) => event.currentTarget.select()}
            />
        </Stack>
    );
};
