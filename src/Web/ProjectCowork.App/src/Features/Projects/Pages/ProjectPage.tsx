import {Skeleton, Stack, Tabs, Text, Title} from "@mantine/core";
import {useProject} from "../../../hooks";
import {Navigate, useParams} from "react-router";
import { RepositoryTab } from "../Components/RepositoryTab.tsx";
import { PostingsTab } from "../../Postings/Components/PostingsTab.tsx";
import { SprintsTab } from "../../Management/Components/SprintsTab.tsx";
import { ProjectAccessTab } from "../../Permissions/ProjectAccess/Components/ProjectAccessTab.tsx";

export const ProjectPage = () => {

    const { projectId } = useParams<{ projectId: string }>();
    const normalizedProjectId = String(projectId ?? '');

    const { data, isLoading, isError } = useProject(normalizedProjectId as string);

    if(isError)
    {
        return (
            <>
                <Navigate to={"/NotFound"} replace={true} />
            </>
        )
    }

    if(isLoading)
    {
        return (
            <>
                <Skeleton visible={isLoading}>
                    Ładowanie....
                </Skeleton>
            </>
        )
    }

    return (
        <>
            <Stack gap="md">
                <Title>{data?.name}</Title>
                <Text>{data?.description}</Text>
                <Tabs defaultValue={"repositories"}>
                    <Tabs.List>
                        <Tabs.Tab value="repositories">Repozytoria</Tabs.Tab>
                        <Tabs.Tab value="sprints">Sprinty i zadania</Tabs.Tab>
                        <Tabs.Tab value="postings">Ogłoszenia</Tabs.Tab>
                        <Tabs.Tab value="access">Członkowie i role</Tabs.Tab>
                    </Tabs.List>
                    <Tabs.Panel value="repositories">
                       <RepositoryTab projectId={normalizedProjectId} />
                    </Tabs.Panel>
                    <Tabs.Panel value="sprints">
                        <SprintsTab projectId={normalizedProjectId} />
                    </Tabs.Panel>
                    <Tabs.Panel value="postings">
                        <PostingsTab projectId={normalizedProjectId} />
                    </Tabs.Panel>
                    <Tabs.Panel value="access">
                        <ProjectAccessTab projectId={normalizedProjectId} />
                    </Tabs.Panel>
                </Tabs>
            </Stack>
        </>
    );
}