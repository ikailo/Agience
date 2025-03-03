import { useEffect } from 'react';
import { useAuth } from '../auth/AuthContext';
import { useLocation, useNavigate } from 'react-router-dom';

const Login = () => {
  const { login, isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const from = location.state?.from?.pathname || '/';

  useEffect(() => {
    if (isAuthenticated) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, from]);

  return (
    <div className="min-h-screen flex items-center justify-center relative overflow-hidden">
      {/* Radial background */}
      <div className="absolute inset-0 bg-gradient-to-br from-gray-400 via-gray-600 to-purple-900 opacity-90"></div>
      
      {/* Radial glow effect */}
      <div className="absolute inset-0">
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-purple-400 rounded-full opacity-20 blur-3xl"></div>
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-purple-300 rounded-full opacity-10 blur-2xl"></div>
      </div>
      
      {/* Background logo */}
      <div className="absolute inset-0 flex items-center justify-center opacity-20">
        <img 
          src="/logo.png" 
          alt="Background Logo" 
          className="w-[800px] max-w-full h-auto animate-pulse-slow"
        />
      </div>
      
      {/* Login card */}
      <div className="max-w-md w-full space-y-8 p-10 bg-white/5 dark:bg-gray-800/10 backdrop-blur-sm rounded-xl shadow-2xl z-10 relative border border-white/20 dark:border-gray-700/20 animate-fadeIn">
        <div className="flex flex-col items-center">
          <img 
            src="/logo.png" 
            alt="Logo" 
            className="h-20 w-auto mb-6 animate-pulse-slow"
          />
          <h2 className="text-center text-3xl font-extrabold text-white dark:text-white">
            Welcome Back
          </h2>
          <p className="mt-2 text-center text-sm text-white/80 dark:text-gray-300">
            Sign in to access your dashboard
          </p>
        </div>
        
        <div className="mt-8">
          <button
            onClick={() => login()}
            className="group relative w-full flex justify-center items-center py-2 px-4 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-lg text-gray-700 dark:text-white bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-all duration-200 shadow-sm hover:shadow-md"
          >
            <span className="absolute left-0 inset-y-0 flex items-center pl-3">
              <svg width="18" height="18" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48">
                <path fill="#EA4335" d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/>
                <path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/>
                <path fill="#FBBC05" d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z"/>
                <path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/>
                <path fill="none" d="M0 0h48v48H0z"/>
              </svg>
            </span>
            <span className="ml-4">Sign in with Google</span>
          </button>
          
          <div className="mt-6 text-center">
            <p className="text-xs text-white/70 dark:text-gray-400">
              By signing in, you agree to our Terms of Service and Privacy Policy
            </p>
          </div>
        </div>
        
        {/* Decorative elements */}
        <div className="absolute top-0 right-0 -mt-4 -mr-4 w-24 h-24 bg-purple-200/50 dark:bg-purple-900/50 rounded-full blur-xl"></div>
        <div className="absolute bottom-0 left-0 -mb-4 -ml-4 w-16 h-16 bg-purple-300/50 dark:bg-purple-800/50 rounded-full blur-xl"></div>
      </div>
      
      {/* Animated particles */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {[...Array(6)].map((_, i) => (
          <div 
            key={i}
            className="absolute rounded-full bg-white opacity-20"
            style={{
              width: `${Math.random() * 8 + 4}px`,
              height: `${Math.random() * 8 + 4}px`,
              top: `${Math.random() * 100}%`,
              left: `${Math.random() * 100}%`,
              animation: `float ${Math.random() * 10 + 10}s linear infinite`
            }}
          ></div>
        ))}
      </div>
    </div>
  );
};

export default Login; 