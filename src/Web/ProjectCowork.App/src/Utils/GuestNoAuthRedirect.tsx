import { Navigate, Outlet } from 'react-router';
import { useAuth } from '../hooks';

export const GuestNoAuthRedirect = () => {
  const { isAuthenticated } = useAuth();

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
};
