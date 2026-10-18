import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  getApiPostingsByProjectIdOptions,
  getApiPostingsByProjectIdQueryKey,
  getApiPostingsPostingByProjectPostingIdOptions,
  getApiPostingsPostingByProjectPostingIdQueryKey,
  postApiPostingsPostingAddMutation,
  postApiPostingsPostingApplyMutation,
  postApiPostingsPostingByProjectPostingIdApplicationsMutation,
  putApiPostingsPostingUpdateMutation,
} from '../client/@tanstack/react-query.gen';

export const usePostings = (projectId: string) =>
  useQuery({
    ...getApiPostingsByProjectIdOptions({ path: { projectId } }),
    enabled: Boolean(projectId),
  });

export const usePosting = (projectPostingId: string) =>
  useQuery({
    ...getApiPostingsPostingByProjectPostingIdOptions({
      path: { projectPostingId },
    }),
    enabled: Boolean(projectPostingId),
  });

export const useAddPosting = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiPostingsPostingAddMutation(),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({
        queryKey: getApiPostingsByProjectIdQueryKey({
          path: { projectId: variables.body.projectId },
        }),
      });
    },
  });
};

export const useUpdatePosting = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...putApiPostingsPostingUpdateMutation(),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({
        queryKey: getApiPostingsPostingByProjectPostingIdQueryKey({
          path: { projectPostingId: variables.body.projectPostingId },
        }),
      });
    },
  });
};

export const useApplyForPosting = () =>
  useMutation(postApiPostingsPostingApplyMutation());

export const useGetPostingApplications = () =>
  useMutation(postApiPostingsPostingByProjectPostingIdApplicationsMutation());
