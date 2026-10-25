import {Skeleton, Stack, Tabs, Text, Title} from "@mantine/core";
import {useProject} from "../../../hooks";
import {Navigate, useParams} from "react-router";
import { RepositoryTab } from "../Components/RepositoryTab.tsx";
import { PostingsTab } from "../../Postings/Components/PostingsTab.tsx";
import { ProjectUsersTab } from "../Components/ProjectUsersTab.tsx";
import { SprintsTab } from "../../Management/Components/SprintsTab.tsx";

export const ProjectPage = () => {

    const { projectId } = useParams<{ projectId: string }>();

    const { data, isLoading, isError } = useProject(projectId);

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
                        <Tabs.Tab value="users">Użytkownicy</Tabs.Tab>
                    </Tabs.List>
                    <Tabs.Panel value="repositories">
                       <RepositoryTab projectId={projectId} />
                    </Tabs.Panel>
                    <Tabs.Panel value="sprints">
                        <SprintsTab projectId={projectId} />
                    </Tabs.Panel>
                    <Tabs.Panel value="postings">
                        <PostingsTab projectId={projectId} />
                    </Tabs.Panel>
                    <Tabs.Panel value="users">
                        <ProjectUsersTab projectId={projectId} />
                    </Tabs.Panel>
                </Tabs>
            </Stack>
        </>
    );
}