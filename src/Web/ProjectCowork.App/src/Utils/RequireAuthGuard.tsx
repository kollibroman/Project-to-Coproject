import { Center, Loader } from '@mantine/core';
import { Navigate, Outlet } from 'react-router';
import { useAuth } from '../hooks';

export const RequireAuthGuard = () => {
  const { isAuthenticated, isLoading } = useAuth();

  // Czekamy na odtworzenie sesji z ciasteczka, zanim zdecydujemy o przekierowaniu.
  if (isLoading) {
    return (
      <Center h="100vh">
        <Loader />
      </Center>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/auth" replace />;
  }

  return <Outlet />;
};
