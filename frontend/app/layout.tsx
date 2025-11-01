import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'
import { ThemeProvider } from '@/components/providers/theme-provider'
import AuthProvider from '@/components/providers/session-provider'
import { ServiceProvider } from '@/lib/providers/ServiceProvider'
import { Navigation } from '@/components/layout/navigation'
import { Toaster } from '@/components/ui/toaster'

const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'CampusEats - Modern Cafeteria Ordering',
  description: 'Order your favorite campus meals with ease',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={inter.className}>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <AuthProvider>
            <ServiceProvider>
              <div className="relative flex min-h-screen flex-col">
                <Navigation />
                <main className="flex-1">{children}</main>
              </div>
              <Toaster />
            </ServiceProvider>
          </AuthProvider>
        </ThemeProvider>
      </body>
    </html>
  )
}

