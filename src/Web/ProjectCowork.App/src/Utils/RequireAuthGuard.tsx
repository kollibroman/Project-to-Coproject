import { Navigate, Outlet } from 'react-router';
import { useAuth } from '../hooks';

export const RequireAuthGuard = () => {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/auth" replace />;
  }

  return <Outlet />;
};
