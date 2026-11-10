import {
    ActionIcon,
    Alert,
    Avatar,
    Badge,
    Button,
    CopyButton,
    Group,
    Paper,
    Skeleton,
    Stack,
    Text,
    Tooltip,
} from "@mantine/core";
import { CheckIcon, CopyIcon, WarningIcon } from "@phosphor-icons/react";
import { useRepositoryCommits } from "../../../hooks";
import type { CommitDto } from "../../../client";
import { formatCommitDate } from "../repositoryUtils";

interface Props {
    repositoryId: string;
    branch: string;
}

const getInitials = (name: string) =>
    name
        .split(" ")
        .map((part) => part[0])
        .join("")
        .slice(0, 2)
        .toUpperCase();

export const RepositoryCommitList = ({ repositoryId, branch }: Props) => {
    const { data, isLoading, isError, hasNextPage, fetchNextPage, isFetchingNextPage } =
        useRepositoryCommits(repositoryId, branch);

    if (isLoading) {
        return (
            <Stack gap="xs">
                {Array.from({ length: 5 }, (_, i) => (
                    <Skeleton key={i} height={64} />
                ))}
            </Stack>
        );
    }

    if (isError) {
        return (
            <Alert color="red" icon={<WarningIcon />}>
                Nie udało się wczytać historii commitów.
            </Alert>
        );
    }

    const commits = data?.pages.flatMap((page) => page.items) ?? [];

    if (commits.length === 0) {
        return (
            <Text c="dimmed" ta="center" py="xl">
                Brak commitów na gałęzi {branch}.
            </Text>
        );
    }

    return (
        <Stack gap="sm">
            <Paper withBorder radius="sm">
                {commits.map((commit, index) => (
                    <CommitRow key={commit.sha} commit={commit} isLast={index === commits.length - 1} />
                ))}
            </Paper>

            {hasNextPage && (
                <Group justify="center">
                    <Button
                        variant="default"
                        loading={isFetchingNextPage}
                        onClick={() => void fetchNextPage()}
                    >
                        Załaduj więcej
                    </Button>
                </Group>
            )}
        </Stack>
    );
};

const CommitRow = ({ commit, isLast }: { commit: CommitDto; isLast: boolean }) => {
    const [title, ...rest] = commit.message.split("\n");
    const description = rest.join("\n").trim();
    const shortSha = commit.sha.slice(0, 7);

    return (
        <Group
            justify="space-between"
            wrap="nowrap"
            px="md"
            py="sm"
            style={isLast ? undefined : { borderBottom: "1px solid var(--mantine-color-default-border)" }}
        >
            <Group gap="sm" wrap="nowrap" style={{ minWidth: 0 }}>
                <Avatar radius="xl" size="md" color="initials" name={commit.authorName}>
                    {getInitials(commit.authorName)}
                </Avatar>
                <Stack gap={2} style={{ minWidth: 0 }}>
                    <Tooltip label={description} disabled={!description} multiline maw={480} withArrow>
                        <Text fw={600} size="sm" truncate="end">
                            {title}
                        </Text>
                    </Tooltip>
                    <Text size="xs" c="dimmed">
                        {commit.authorName} · {formatCommitDate(commit.date)}
                    </Text>
                </Stack>
            </Group>

            <Group gap={4} wrap="nowrap">
                <Badge variant="light" color="gray" ff="monospace" tt="none">
                    {shortSha}
                </Badge>
                <CopyButton value={commit.sha} timeout={2000}>
                    {({ copied, copy }) => (
                        <Tooltip label={copied ? "Skopiowano" : "Kopiuj pełny SHA"} withArrow>
                            <ActionIcon variant="subtle" color={copied ? "teal" : "gray"} onClick={copy}>
                                {copied ? <CheckIcon /> : <CopyIcon />}
                            </ActionIcon>
                        </Tooltip>
                    )}
                </CopyButton>
            </Group>
        </Group>
    );
};
