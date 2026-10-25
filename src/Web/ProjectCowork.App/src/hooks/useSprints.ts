import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  deleteApiSprintsBySprintIdMutation,
  getApiSprintsProjectByProjectIdOptions,
  getApiSprintsProjectByProjectIdQueryKey,
  postApiSprintsMutation,
  putApiSprintsBySprintIdMutation,
} from '../client/@tanstack/react-query.gen';

export const useSprints = (projectId: string | undefined) =>
  useQuery({
    ...getApiSprintsProjectByProjectIdOptions({ path: { projectId: projectId ?? '' } }),
    enabled: Boolean(projectId),
  });

export const useCreateSprint = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiSprintsMutation(),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({
        queryKey: getApiSprintsProjectByProjectIdQueryKey({
          path: { projectId: variables.body.projectId },
        }),
      });
    },
  });
};

export const useEditSprint = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...putApiSprintsBySprintIdMutation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: [{ _id: 'getApiSprintsProjectByProjectId' }],
        exact: false,
      });
      void queryClient.invalidateQueries({
        queryKey: [{ _id: 'getApiSprintsBySprintId' }],
        exact: false,
      });
    },
  });
};

export const useDeleteSprint = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...deleteApiSprintsBySprintIdMutation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: [{ _id: 'getApiSprintsProjectByProjectId' }],
        exact: false,
      });
      // Zadania usuniętego sprintu wracają do backlogu, więc listy zadań też są nieaktualne.
      void queryClient.invalidateQueries({
        queryKey: [{ _id: 'getApiTasksProjectByProjectId' }],
        exact: false,
      });
      void queryClient.invalidateQueries({
        queryKey: [{ _id: 'getApiSprintsBySprintIdTasks' }],
        exact: false,
      });
    },
  });
};
