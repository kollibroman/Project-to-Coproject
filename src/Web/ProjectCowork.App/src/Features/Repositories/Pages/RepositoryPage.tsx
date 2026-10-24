import {
    ActionIcon,
    Button,
    CopyButton,
    Group,
    Popover,
    Skeleton,
    Stack,
    Text,
    TextInput,
    Title,
    Tooltip,
} from "@mantine/core";
import { CheckIcon, CodeIcon, CopyIcon } from "@phosphor-icons/react";
import { Navigate, useParams } from "react-router";
import { useRepository } from "../../../hooks";

export const RepositoryPage = () => {
    const { repositoryId } = useParams<{ repositoryId: string }>();

    const { data, isLoading, isError } = useRepository(repositoryId ?? "");

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
        </Stack>
    );
};
