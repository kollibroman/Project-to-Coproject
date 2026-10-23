import {ActionIcon, Flex, Grid, Paper, Stack, Text, Title} from "@mantine/core";
import type {ProjectModel} from "../../client";
import {ArrowRightIcon} from "@phosphor-icons/react";
import {useDisclosure} from "@mantine/hooks";

interface Props {
    project: ProjectModel;
}

export const ProjectUserPaper = ({ project: { id, name, description } }: Props) => {

    return (
        <>
            <Paper withBorder shadow="md" radius="sm" p="xl">
                <Grid>
                    <Grid.Col span={11}>
                        <Stack>
                            <Grid>
                                <Grid.Col span={9}>
                                    <Title order={1}>{name}</Title>
                                </Grid.Col>
                                <Grid.Col span={3}>
                                    // TODO: wpisywać zwracana role w projekcie
                                    <Text>Rola: </Text>
                                </Grid.Col>
                            </Grid>
                            <Text size="sm">{description}</Text>
                        </Stack>
                    </Grid.Col>
                    <Grid.Col span={1}>
                        <ActionIcon variant="filled" size="lg" radius="sm">
                            <ArrowRightIcon style={{ width: '70%', height: '70%' }}/>
                        </ActionIcon>
                    </Grid.Col>
                </Grid>
            </Paper>
        </>
    );
}