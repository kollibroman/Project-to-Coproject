import type {RepositoryDto} from "../../../client";
import {ActionIcon, Grid, Paper, Stack, Title} from "@mantine/core";
import {Link} from "react-router";
import {ArrowRightIcon} from "@phosphor-icons/react";

interface Props {
    repository: RepositoryDto;
}

export const RepositoryPaper = ({repository: { id, name }}: Props) => {

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
                            </Grid>
                        </Stack>
                    </Grid.Col>
                    <Grid.Col span={1}>
                        <ActionIcon variant="filled" size="lg" radius="sm" component={Link} to={`/repository/${id}`}>
                            <ArrowRightIcon style={{ width: '70%', height: '70%' }}/>
                        </ActionIcon>
                    </Grid.Col>
                </Grid>
            </Paper>
        </>
    );
}