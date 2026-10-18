import { useSyncExternalStore } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  postApiAuthLoginMutation,
  postApiAuthRegisterBusinessMutation,
  postApiAuthRegisterUserMutation,
} from '../client/@tanstack/react-query.gen';
import { getToken, setToken, subscribe } from '../auth/session';

export const useAuth = () => {
  const token = useSyncExternalStore(subscribe, getToken, getToken);

  return {
    token,
    isAuthenticated: Boolean(token),
    setToken,
    logout: () => setToken(null),
  };
};

export const useLogin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    ...postApiAuthLoginMutation(),
    onSuccess: (data) => {
      if (data?.accessToken) {
        setToken(data.accessToken);
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
        setToken(data.accessToken);
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
        setToken(data.accessToken);
      }
      void queryClient.invalidateQueries();
    },
  });
};
