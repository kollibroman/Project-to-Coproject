import { type QueryClient, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  deleteApiTasksByTaskIdMutation,
  getApiSprintsBySprintIdTasksOptions,
  getApiTasksProjectByProjectIdOptions,
  postApiTasksMutation,
  putApiTasksByTaskIdMutation,
} from '../client/@tanstack/react-query.gen';

const invalidateTaskLists = (queryClient: QueryClient) => {
  void queryClient.invalidateQueries({
    queryKey: [{ _id: 'getApiTasksProjectByProjectId' }],
    exact: false,
  });
  void queryClient.invalidateQueries({
    queryKey: [{ _id: 'getApiSprintsBySprintIdTasks' }],
    exact: false,
  });
  void queryClient.invalidateQueries({
    queryKey: [{ _id: 'getApiTasksByTaskId' }],
    exact: false,
  });
};

export const useProjectTasks = (projectId: string | undefined) =>
  useQuery({
    ...getApiTasksProjectByProjectIdOptions({ path: { projectId: projectId ?? '' } }),
    enabled: Boolean(projectId),
  });

export const useSprintTasks = (sprintId: string | undefined) =>
  useQuery({
    ...getApiSprintsBySprintIdTasksOptions({ path: { sprintId: sprintId ?? '' } }),
    enabled: Boolean(sprintId),
  });

export const useCreateTask = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiTasksMutation(),
    onSuccess: () => invalidateTaskLists(queryClient),
  });
};

export const useEditTask = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...putApiTasksByTaskIdMutation(),
    onSuccess: () => invalidateTaskLists(queryClient),
  });
};

export const useDeleteTask = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...deleteApiTasksByTaskIdMutation(),
    onSuccess: () => invalidateTaskLists(queryClient),
  });
};
