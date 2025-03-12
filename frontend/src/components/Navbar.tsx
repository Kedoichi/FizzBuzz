import Link from "next/link";

export default function Navbar() {
  return (
    <header className="bg-white shadow">
      <div className="container mx-auto px-4">
        <div className="flex justify-between items-center h-16">
          <div className="flex items-center">
            <Link href="/" className="flex items-center">
              <span className="text-2xl font-bold text-blue-600">FizzBuzz</span>
              <span className="ml-1 text-2xl font-bold text-gray-700">
                Game
              </span>
            </Link>
          </div>

          <nav className="flex items-center space-x-8">
            <Link href="/" className="py-2 border-b-2 hover:border-gray-300">
              Home
            </Link>

            <Link
              href="/create"
              className="py-2 border-b-2 hover:border-gray-300"
            >
              Create Game
            </Link>
          </nav>
        </div>
      </div>
    </header>
  );
}
