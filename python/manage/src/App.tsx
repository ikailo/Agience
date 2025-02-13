import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Layout } from './components/layout/Layout';
import Home from './pages/Home';
import Agent from './pages/Agent';
import Topics from './pages/Topics';
import Hosts from './pages/Hosts';
import Plugins from './pages/Plugins';

function App() {
  return (
    <Router>
      <Layout>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/agent" element={<Agent />} />
          <Route path="/topics" element={<Topics />} />
          <Route path="/hosts" element={<Hosts />} />
          <Route path="/plugins" element={<Plugins />} />
        </Routes>
      </Layout>
    </Router>
  );
}

export default App;
