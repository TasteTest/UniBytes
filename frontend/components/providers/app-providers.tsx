/**
 * App Providers
 * Centralized provider composition
 * Following Composition over Inheritance
 */

'use client'

import { SessionProvider } from 'next-auth/react'
import { ThemeProvider } from 'next-themes'
import { ServiceProvider } from '@/lib/providers/ServiceProvider'

interface AppProvidersProps {
  children: React.ReactNode
}

export function AppProviders({ children }: AppProvidersProps) {
  return (
    <SessionProvider>
      <ServiceProvider>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          {children}
        </ThemeProvider>
      </ServiceProvider>
    </SessionProvider>
  )
}

