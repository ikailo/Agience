import { useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { UserManager } from 'oidc-client-ts';
import { AuthConfig } from '../auth/AuthConfig';
import { Loading } from '../components/common/Loading';

const AuthCallback = () => {
  const navigate = useNavigate();
  const callbackHandled = useRef(false);

  useEffect(() => {
    const handleCallback = async () => {
      if (callbackHandled.current) return;
      callbackHandled.current = true;
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