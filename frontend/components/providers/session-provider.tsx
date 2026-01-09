"use client"

import { SessionProvider, useSession } from "next-auth/react"
import { useEffect } from "react"
import { apiClient } from "@/lib/api/client"

/**
 * Inner component that syncs session to API client
 */
function AuthSync({ children }: { children: React.ReactNode }) {
  const { data: session, status } = useSession()

  useEffect(() => {
    if (status === "authenticated" && session?.accessToken && session?.user?.email) {
      // Set auth headers on the API client
      apiClient.setAuth(session.accessToken, session.user.email)
    } else if (status === "unauthenticated") {
      // Remove auth when logged out
      apiClient.removeAuth()
    }
  }, [session, status])

  return <>{children}</>
}

export default function AuthProvider({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <SessionProvider>
      <AuthSync>{children}</AuthSync>
    </SessionProvider>
  )
}

