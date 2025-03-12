import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function RootLayout() {
  return (
    <div className="min-h-screen flex flex-col bg-gray-50">
      <Navbar />
      
      <main className="flex-grow container mx-auto px-4 py-6">
        <Outlet />
      </main>
      
      <footer className="bg-white border-t border-gray-200 py-4 text-center text-gray-600">
        <div className="container mx-auto px-4">
          <p>© {new Date().getFullYear()} FizzBuzz Game - A fun brain training exercise</p>
        </div>
      </footer>
    </div>
  );
}