import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { UserManager } from 'oidc-client-ts';
import { AuthConfig } from '../auth/AuthConfig';
import { Loading } from '../components/common/Loading';

const AuthCallback = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const handleCallback = async () => {
      const userManager = new UserManager(AuthConfig);
      
      try {
        await userManager.signinRedirectCallback();
        window.location.href = '/';
      } catch (error) {
        console.error('Signin error:', error);
        navigate('/login', { replace: true });
      }
    };

    handleCallback();
  }, [navigate]);

  return <Loading />;
};

export default AuthCallback; 