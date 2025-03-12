"use client";

import { BrowserRouter } from "react-router-dom";
import "./globals.css";
import Navbar from "@/components/Navbar";
import { Inter } from "next/font/google";

const inter = Inter({ subsets: ["latin"] });

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <>
      <html lang="en">
        <body className={inter.className}>
          <BrowserRouter>
            <div className="min-h-screen flex flex-col bg-gray-50">
              <Navbar />

              {/* Main content */}
              <main className="flex-grow container mx-auto px-4 py-6">
                {children}
              </main>

              {/* Footer */}
              <footer className="bg-white border-t border-gray-200 py-4 text-center text-gray-600">
                <div className="container mx-auto px-4">
                  <p>FizzBuzz Game - by quocchic</p>
                </div>
              </footer>
            </div>
          </BrowserRouter>
        </body>
      </html>
    </>
  );
}
