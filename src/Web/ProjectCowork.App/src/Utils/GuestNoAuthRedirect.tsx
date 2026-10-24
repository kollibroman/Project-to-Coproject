import { Center, Loader } from '@mantine/core';
import { Navigate, Outlet } from 'react-router';
import { useAuth } from '../hooks';

export const GuestNoAuthRedirect = () => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <Center h="100vh">
        <Loader />
      </Center>
    );
  }

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
};
