import { Alert, Center, Divider, Loader, Stack } from '@mantine/core';
import {
    useChangeProjectMemberRole,
    usePermissions,
    useProjectMembers,
    useProjectRoles,
    useCreateProjectRole,
    useDeleteProjectRole,
    useRemoveProjectMember,
    useUpdateProjectRole,
} from '../../../../hooks';
import { useInviteUser } from '../../../../hooks';
import { ProjectMembersSection } from './ProjectMembersSection.tsx';
import { ProjectRolesSection } from './ProjectRolesSection.tsx';
import type { ProjectMember, ProjectRole } from '../projectAccessTypes.ts';

type Props = {
    projectId: string | undefined;
};

export const ProjectAccessTab = ({ projectId }: Props) => {
    const rolesQuery = useProjectRoles(projectId);
    const membersQuery = useProjectMembers(projectId);
    const permissionsQuery = usePermissions();
    const createRole = useCreateProjectRole(projectId ?? '');
    const updateRole = useUpdateProjectRole(projectId ?? '');
    const deleteRole = useDeleteProjectRole(projectId ?? '');
    const inviteMember = useInviteUser();
    const changeMemberRole = useChangeProjectMemberRole(projectId ?? '');
    const removeMember = useRemoveProjectMember(projectId ?? '');

    if (!projectId || rolesQuery.isLoading || membersQuery.isLoading || permissionsQuery.isLoading) {
        return (
            <Center mih={180}>
                <Loader />
            </Center>
        );
    }

    if (rolesQuery.isError || membersQuery.isError || permissionsQuery.isError) {
        return <Alert color="red" title="Błąd">Nie udało się pobrać ról i członków projektu.</Alert>;
    }

    const catalog = (permissionsQuery.data ?? []).map((permission) => ({
        ...permission,
        id: Number(permission.id),
    }));
    const roles: ProjectRole[] = (rolesQuery.data ?? []).map((role) => ({
        ...role,
        permissionIds: role.permissionIds.map(Number),
        memberCount: Number(role.memberCount),
    }));
    const members: ProjectMember[] = (membersQuery.data ?? []).map((member) => ({ ...member }));

    return (
        <Stack gap="xl" pt="md">
            <ProjectRolesSection
                roles={roles}
                catalog={catalog}
                onCreate={(input) => createRole.mutateAsync({ path: { projectId }, body: input }).then(() => undefined)}
                onUpdate={(roleId, input) =>
                    updateRole.mutateAsync({ path: { projectId, roleId }, body: input }).then(() => undefined)
                }
                onDelete={(roleId) =>
                    deleteRole.mutateAsync({ path: { projectId, roleId } }).then(() => undefined)
                }
            />

            <Divider />

            <ProjectMembersSection
                members={members}
                roles={roles}
                onInvite={(email, roleId) =>
                    inviteMember.mutate({
                        path: { projectId },
                        body: { projectId, email, roleId },
                    })
                }
                onChangeRole={(userId, roleId) =>
                    changeMemberRole.mutate({ path: { projectId, userId }, body: { roleId } })
                }
                onRemove={(userId) => removeMember.mutate({ path: { projectId, userId } })}
            />
        </Stack>
    );
};
