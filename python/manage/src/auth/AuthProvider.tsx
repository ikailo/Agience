import { useState, useEffect, ReactNode } from 'react';
import { User, UserManager } from 'oidc-client-ts';
import { AuthContext } from './AuthContext';
import { AuthConfig } from './AuthConfig';
import { useNavigate } from 'react-router-dom';

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [userManager] = useState(new UserManager(AuthConfig));
  const navigate = useNavigate();

  useEffect(() => {
    const getUser = async () => {
      try {
        const currentUser = await userManager.getUser();
        setUser(currentUser);
      } catch (error) {
        console.error('Error getting user:', error);
      } finally {
        setIsLoading(false);
      }
    };

    getUser();

    // Subscribe to user changes
    const handleUserLoaded = (loadedUser: User) => {
      setUser(loadedUser);
      setIsLoading(false);
    };

    const handleUserUnloaded = () => {
      setUser(null);
      setIsLoading(false);
      navigate('/login');
    };

    userManager.events.addUserLoaded(handleUserLoaded);
    userManager.events.addUserUnloaded(handleUserUnloaded);

    return () => {
      userManager.events.removeUserLoaded(handleUserLoaded);
      userManager.events.removeUserUnloaded(handleUserUnloaded);
    };
  }, [userManager, navigate]);

  const login = async () => {
    try {
      await userManager.signinRedirect();
    } catch (error) {
      console.error('Login error:', error);
    }
  };

  const logout = async () => {
    try {
      const user = await userManager.getUser();
      if (user?.access_token) {
        await userManager.revokeTokens();
      }
      await userManager.removeUser();
      window.location.href = '/login';
    } catch (error) {
      console.error('Logout error:', error);
      await userManager.removeUser();
      window.location.href = '/login';
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}; 