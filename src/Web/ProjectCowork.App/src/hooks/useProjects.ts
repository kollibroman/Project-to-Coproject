import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  deleteApiProjectsByProjectIdMutation,
  getApiProjectsByProjectIdOptions,
  getApiProjectsByProjectIdQueryKey,
  getApiProjectsDiscoverOptions,
  getApiProjectsOptions,
  getApiProjectsQueryKey,
  postApiProjectsByProjectIdEditMutation,
  postApiProjectsByProjectIdInviteMutation,
  postApiProjectsMutation,
} from '../client/@tanstack/react-query.gen';
import type { Options } from '../client';
import type { GetApiProjectsData } from '../client';

export const useProjects = (options?: Options<GetApiProjectsData>) =>
  useQuery(getApiProjectsOptions(options));

export const useProject = (projectId: string) =>
  useQuery({
    ...getApiProjectsByProjectIdOptions({ path: { projectId } }),
    enabled: Boolean(projectId),
  });

export const useDiscoverProjects = () =>
  useQuery(getApiProjectsDiscoverOptions());

export const useCreateProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiProjectsMutation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: getApiProjectsQueryKey(),
      });
    },
  });
};

/**
 * Edycja projektu. POST /api/projects/{projectId}/edit
 * Po sukcesie unieważnia listę i szczegóły projektu.
 */
export const useEditProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiProjectsByProjectIdEditMutation(),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({
        queryKey: getApiProjectsQueryKey(),
      });
      void queryClient.invalidateQueries({
        queryKey: getApiProjectsByProjectIdQueryKey({
          path: { projectId: variables.body.projectId },
        }),
      });
    },
  });
};

export const useInviteUser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiProjectsByProjectIdInviteMutation(),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({
        queryKey: getApiProjectsByProjectIdQueryKey({
          path: { projectId: variables.path.projectId },
        }),
      });
    },
  });
};

/**
 * Usunięcie projektu. DELETE /api/projects/{projectId}
 * Po sukcesie unieważnia listę projektów.
 */
export const useDeleteProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...deleteApiProjectsByProjectIdMutation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: getApiProjectsQueryKey(),
      });
    },
  });
};
