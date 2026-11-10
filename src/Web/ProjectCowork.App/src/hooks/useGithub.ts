import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  getApiGithubStatusOptions,
  getApiGithubStatusQueryKey,
  getApiGithubUserRepositoriesOptions,
  getApiGithubUserRepositoriesQueryKey,
  getApiRepositoryProjectByProjectIdQueryKey,
  postApiGithubAuthorizeMutation,
  postApiGithubRepositoryImportMutation,
} from '../client/@tanstack/react-query.gen';

export const useGithubStatus = () => useQuery(getApiGithubStatusOptions());

export const useConnectGithub = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiGithubAuthorizeMutation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: getApiGithubStatusQueryKey() });
      void queryClient.invalidateQueries({ queryKey: getApiGithubUserRepositoriesQueryKey() });
    },
  });
};

export const useGithubRepositories = (enabled: boolean) =>
  useQuery({
    ...getApiGithubUserRepositoriesOptions(),
    enabled,
  });

export const useImportGithubRepository = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiGithubRepositoryImportMutation(),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({
        queryKey: getApiRepositoryProjectByProjectIdQueryKey({
          path: { projectId: variables.body.projectId },
        }),
      });
    },
  });
};
