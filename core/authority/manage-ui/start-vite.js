import net from 'net';
import { spawn } from 'child_process';

const PORT = 5002;

function checkPort(port) {
  return new Promise((resolve, reject) => {
    const server = net.createServer();
    server.once('error', (err) => {
      if (err.code === 'EADDRINUSE') {
        // Port is in use.
        resolve(false);
      } else {
        reject(err);
      }
    });
    server.once('listening', () => {
      server.close();
      resolve(true);
    });
    server.listen(port, '127.0.0.1');
  });
}

(async function() {
  try {
    const isAvailable = await checkPort(PORT);
    if (isAvailable) {
      console.log(`Starting Vite on port ${PORT}...`);
      // Since you're running from the directory with vite available in PATH, call it directly.
      const vite = spawn('vite', [], { stdio: 'inherit', shell: true });
      vite.on('exit', (code) => process.exit(code));
      vite.on('error', (err) => {
        console.error('Error starting Vite:', err);
        process.exit(1);
      });
      console.log(`Started Vite on port ${PORT}`);
    } else {
      console.log(`Vite is already running on port ${PORT}`);
    }
  } catch (err) {
    console.error(err);
    process.exit(1);
  }
})();
