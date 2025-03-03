import { Link } from 'react-router-dom';
import { Button } from '../components/common/Button';

export default function Home() {
  return (
    <div className="h-[calc(100vh-4rem)] flex items-center justify-center px-4">
      <div className="max-w-4xl mx-auto space-y-6 text-center">
        {/* Main Heading */}
        <div className="space-y-2">
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold text-gray-900 dark:text-white">
            AI Agents Powered by You
          </h1>
          <p className="text-xl sm:text-2xl text-gray-600 dark:text-gray-300">
            The open agentic platform & ecosystem for everyone
          </p>
        </div>

        {/* Description */}
        <p className="text-lg sm:text-xl text-gray-700 dark:text-gray-400 max-w-3xl mx-auto">
          Agience activates humanity's potential in AI with a secure, distributed 
          architecture enhancing privacy, scalability, and efficiency while 
          empowering people to innovate and earn rewards.
        </p>

        {/* CTA Button */}
        <div className="mt-8">
          <Link to="/agent">
            <Button 
              variant="primary"
              className="text-lg px-8 py-3 font-semibold"
            >
              Get Started
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
