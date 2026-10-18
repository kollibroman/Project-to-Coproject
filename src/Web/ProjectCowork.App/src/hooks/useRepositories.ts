import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  deleteApiRepositoryByRepositoryIdMutation,
  getApiRepositoryByRepositoryIdOptions,
  getApiRepositoryProjectByProjectIdOptions,
  getApiRepositoryProjectByProjectIdQueryKey,
  postApiGitRepositoryCreateMutation,
} from '../client/@tanstack/react-query.gen';

export const useRepositories = (projectId: string) =>
  useQuery({
    ...getApiRepositoryProjectByProjectIdOptions({ path: { projectId } }),
    enabled: Boolean(projectId),
  });

export const useRepository = (repositoryId: string) =>
  useQuery({
    ...getApiRepositoryByRepositoryIdOptions({ path: { repositoryId } }),
    enabled: Boolean(repositoryId),
  });

export const useCreateRepository = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiGitRepositoryCreateMutation(),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({
        queryKey: getApiRepositoryProjectByProjectIdQueryKey({
          path: { projectId: variables.body.projectId },
        }),
      });
    },
  });
};

export const useDeleteRepository = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...deleteApiRepositoryByRepositoryIdMutation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: [{ _id: 'getApiRepositoryProjectByProjectId' }],
        exact: false,
      });
    },
  });
};
