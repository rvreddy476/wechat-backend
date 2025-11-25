import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import { Toaster } from 'react-hot-toast'
import './globals.css'
import { ReduxProvider } from '@/store/Provider'

const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'WeChat - Connect with the World',
  description: 'Social media platform for sharing posts, videos, and chatting with friends',
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={inter.className}>
        <ReduxProvider>
          {children}
          <Toaster
            position="top-right"
            toastOptions={{
              duration: 4000,
              style: {
                background: '#fff',
                color: '#333',
              },
              success: {
                duration: 3000,
                iconTheme: {
                  primary: '#E30C4D',
                  secondary: '#fff',
                },
              },
              error: {
                duration: 4000,
              },
            }}
          />
        </ReduxProvider>
      </body>
    </html>
  )
}
