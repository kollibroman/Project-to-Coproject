import { useMutation, useQuery, useQueryClient, type QueryClient } from '@tanstack/react-query';
import {
    getApiPermissionsQueryKey,
    getApiProjectsByProjectIdMembersOptions,
    getApiProjectsByProjectIdMembersQueryKey,
    getApiProjectsByProjectIdRolesOptions,
    getApiProjectsByProjectIdRolesQueryKey,
    postApiProjectsByProjectIdRolesMutation,
    putApiProjectsByProjectIdMembersByUserIdMutation,
    putApiProjectsByProjectIdRolesByRoleIdMutation,
    deleteApiProjectsByProjectIdMembersByUserIdMutation,
    deleteApiProjectsByProjectIdRolesByRoleIdMutation,
} from '../client/@tanstack/react-query.gen';
import {
    getApiPermissions,
} from '../client';

export const usePermissions = () =>
    useQuery({
        queryKey: getApiPermissionsQueryKey(),
        queryFn: ({ signal }) => getApiPermissions({ signal }).then(({ data }) => data),
    });

export const useProjectRoles = (projectId: string | undefined) =>
    useQuery({
        ...getApiProjectsByProjectIdRolesOptions({ path: { projectId: projectId ?? '' } }),
        enabled: Boolean(projectId),
    });

export const useProjectMembers = (projectId: string | undefined) =>
    useQuery({
        ...getApiProjectsByProjectIdMembersOptions({ path: { projectId: projectId ?? '' } }),
        enabled: Boolean(projectId),
    });

const invalidateProjectAccess = (queryClient: QueryClient, projectId: string) => {
    void queryClient.invalidateQueries({
        queryKey: getApiProjectsByProjectIdRolesQueryKey({ path: { projectId } }),
    });
    void queryClient.invalidateQueries({
        queryKey: getApiProjectsByProjectIdMembersQueryKey({ path: { projectId } }),
    });
};

export const useCreateProjectRole = (projectId: string) => {
    const queryClient = useQueryClient();
    return useMutation({
        ...postApiProjectsByProjectIdRolesMutation(),
        onSuccess: () => invalidateProjectAccess(queryClient, projectId),
    });
};

export const useUpdateProjectRole = (projectId: string) => {
    const queryClient = useQueryClient();
    return useMutation({
        ...putApiProjectsByProjectIdRolesByRoleIdMutation(),
        onSuccess: () => invalidateProjectAccess(queryClient, projectId),
    });
};

export const useDeleteProjectRole = (projectId: string) => {
    const queryClient = useQueryClient();
    return useMutation({
        ...deleteApiProjectsByProjectIdRolesByRoleIdMutation(),
        onSuccess: () => invalidateProjectAccess(queryClient, projectId),
    });
};

export const useChangeProjectMemberRole = (projectId: string) => {
    const queryClient = useQueryClient();
    return useMutation({
        ...putApiProjectsByProjectIdMembersByUserIdMutation(),
        onSuccess: () => invalidateProjectAccess(queryClient, projectId),
    });
};

export const useRemoveProjectMember = (projectId: string) => {
    const queryClient = useQueryClient();
    return useMutation({
        ...deleteApiProjectsByProjectIdMembersByUserIdMutation(),
        onSuccess: () => invalidateProjectAccess(queryClient, projectId),
    });
};