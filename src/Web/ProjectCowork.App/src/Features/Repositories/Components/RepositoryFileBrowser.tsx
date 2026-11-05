import {
    Alert,
    Anchor,
    Breadcrumbs,
    Group,
    Paper,
    Skeleton,
    Stack,
    Table,
    Text,
} from "@mantine/core";
import { CodeHighlight } from "@mantine/code-highlight";
import { FileIcon, FolderIcon, FolderOpenIcon, WarningIcon } from "@phosphor-icons/react";
import { useMemo } from "react";
import { useRepositoryFile, useRepositoryTree } from "../../../hooks";
import {
    formatFileSize,
    getFileName,
    getLanguageFromPath,
    getParentPath,
} from "../repositoryUtils";

interface Props {
    repositoryId: string;
    repositoryName: string;
    branch: string;
    /** Ścieżka aktualnego folderu ("" = katalog główny). */
    path: string;
    /** Ścieżka otwartego pliku albo null, gdy wyświetlana jest lista plików. */
    filePath: string | null;
    onOpenFolder: (path: string) => void;
    onOpenFile: (path: string) => void;
}

export const RepositoryFileBrowser = ({
    repositoryId,
    repositoryName,
    branch,
    path,
    filePath,
    onOpenFolder,
    onOpenFile,
}: Props) => {
    const currentFolder = filePath ? getParentPath(filePath) : path;
    const breadcrumbPath = filePath ?? path;

    const breadcrumbs = useMemo(() => {
        const segments = breadcrumbPath ? breadcrumbPath.split("/") : [];
        return [
            { label: repositoryName, path: "", isFile: false },
            ...segments.map((segment, index) => ({
                label: segment,
                path: segments.slice(0, index + 1).join("/"),
                isFile: Boolean(filePath) && index === segments.length - 1,
            })),
        ];
    }, [breadcrumbPath, filePath, repositoryName]);

    return (
        <Stack gap="sm">
            <Breadcrumbs separator="/">
                {breadcrumbs.map((crumb, index) =>
                    index === breadcrumbs.length - 1 ? (
                        <Text key={crumb.path} fw={600}>
                            {crumb.label}
                        </Text>
                    ) : (
                        <Anchor
                            key={crumb.path}
                            component="button"
                            type="button"
                            onClick={() => onOpenFolder(crumb.path)}
                        >
                            {crumb.label}
                        </Anchor>
                    ),
                )}
            </Breadcrumbs>

            {filePath ? (
                <FileView repositoryId={repositoryId} branch={branch} filePath={filePath} />
            ) : (
                <TreeView
                    repositoryId={repositoryId}
                    branch={branch}
                    path={currentFolder}
                    onOpenFolder={onOpenFolder}
                    onOpenFile={onOpenFile}
                />
            )}
        </Stack>
    );
};

interface TreeViewProps {
    repositoryId: string;
    branch: string;
    path: string;
    onOpenFolder: (path: string) => void;
    onOpenFile: (path: string) => void;
}

const TreeView = ({ repositoryId, branch, path, onOpenFolder, onOpenFile }: TreeViewProps) => {
    const { data, isLoading, isError } = useRepositoryTree(repositoryId, branch, path);

    if (isLoading) {
        return <Skeleton height={240} />;
    }

    if (isError) {
        return (
            <Alert color="red" icon={<WarningIcon />}>
                Nie udało się wczytać zawartości folderu.
            </Alert>
        );
    }

    // Backend zwraca już foldery przed plikami, alfabetycznie.
    const entries = data ?? [];

    return (
        <Paper withBorder radius="sm">
            <Table highlightOnHover verticalSpacing="xs">
                <Table.Tbody>
                    {path && (
                        <Table.Tr
                            style={{ cursor: "pointer" }}
                            onClick={() => onOpenFolder(getParentPath(path))}
                        >
                            <Table.Td colSpan={2}>
                                <Group gap="xs">
                                    <FolderOpenIcon />
                                    <Text size="sm">..</Text>
                                </Group>
                            </Table.Td>
                        </Table.Tr>
                    )}

                    {entries.length === 0 && (
                        <Table.Tr>
                            <Table.Td colSpan={2}>
                                <Text size="sm" c="dimmed" ta="center" py="md">
                                    {path ? "Ten folder jest pusty." : "Repozytorium jest puste. Wypchnij pierwszy commit, aby zobaczyć pliki."}
                                </Text>
                            </Table.Td>
                        </Table.Tr>
                    )}

                    {entries.map((entry) => (
                        <Table.Tr
                            key={entry.path}
                            style={{ cursor: "pointer" }}
                            onClick={() =>
                                entry.type === "folder" ? onOpenFolder(entry.path) : onOpenFile(entry.path)
                            }
                        >
                            <Table.Td>
                                <Group gap="xs" wrap="nowrap">
                                    {entry.type === "folder" ? (
                                        <FolderIcon weight="fill" color="var(--mantine-color-blue-4)" />
                                    ) : (
                                        <FileIcon />
                                    )}
                                    <Text size="sm">{entry.name}</Text>
                                </Group>
                            </Table.Td>
                            <Table.Td w={120} ta="right">
                                <Text size="sm" c="dimmed">
                                    {entry.type === "file" ? formatFileSize(entry.size) : ""}
                                </Text>
                            </Table.Td>
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
        </Paper>
    );
};

interface FileViewProps {
    repositoryId: string;
    branch: string;
    filePath: string;
}

const FileView = ({ repositoryId, branch, filePath }: FileViewProps) => {
    const { data, isLoading, isError } = useRepositoryFile(repositoryId, branch, filePath);

    if (isLoading) {
        return <Skeleton height={320} />;
    }

    if (isError || !data) {
        return (
            <Alert color="red" icon={<WarningIcon />}>
                Nie udało się wczytać pliku.
            </Alert>
        );
    }

    return (
        <Paper withBorder radius="sm">
            <Group justify="space-between" px="md" py="xs">
                <Text size="sm" fw={600}>
                    {getFileName(data.path)}
                </Text>
                <Text size="sm" c="dimmed">
                    {formatFileSize(data.size)}
                </Text>
            </Group>

            {data.isBinary || data.content == null ? (
                <Text size="sm" c="dimmed" ta="center" py="xl">
                    Plik binarny lub zbyt duży, aby go wyświetlić.
                </Text>
            ) : (
                <CodeHighlight
                    code={data.content}
                    language={getLanguageFromPath(data.path)}
                    withLineNumbers
                    radius={0}
                />
            )}
        </Paper>
    );
};
