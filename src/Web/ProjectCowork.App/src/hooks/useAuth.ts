import { useSyncExternalStore } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  postApiAuthLoginMutation,
  postApiAuthRegisterBusinessMutation,
  postApiAuthRegisterUserMutation,
} from '../client/@tanstack/react-query.gen';
import { getAuthState, logout, setAccessToken, subscribe } from '../auth/session';

export const useAuth = () => {
  const { status, token } = useSyncExternalStore(subscribe, getAuthState, getAuthState);
  const queryClient = useQueryClient();

  return {
    token,
    status,
    isAuthenticated: status === 'authenticated',
    isLoading: status === 'unknown',
    logout: async () => {
      await logout();
      queryClient.clear();
    },
  };
};

export const useLogin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiAuthLoginMutation(),
    onSuccess: (data) => {
      if (data?.accessToken) {
        setAccessToken(data.accessToken);
      }
      void queryClient.invalidateQueries();
    },
  });
};

export const useRegisterUser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiAuthRegisterUserMutation(),
    onSuccess: (data) => {
      if (data?.accessToken) {
        setAccessToken(data.accessToken);
      }
      void queryClient.invalidateQueries();
    },
  });
};

export const useRegisterBusiness = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiAuthRegisterBusinessMutation(),
    onSuccess: (data) => {
      if (data?.accessToken) {
        setAccessToken(data.accessToken);
      }
      void queryClient.invalidateQueries();
    },
  });
};
