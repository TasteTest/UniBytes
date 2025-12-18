/**
 * Role Hook
 * Provides easy access to user role from session
 * Role values: 0=User, 1=Chef, 2=Admin
 */

'use client'

import { useSession } from 'next-auth/react'

interface UseRoleReturn {
    role: number // 0=User, 1=Chef, 2=Admin
    isUser: boolean
    isChef: boolean
    isAdmin: boolean
    canAccessKitchen: boolean // Chef only (Admin manages users/menu)
    canOrder: boolean // Only User
    isLoading: boolean
}

export const useRole = (): UseRoleReturn => {
    const { data: session, status } = useSession()
    const role = session?.user?.role ?? 0

    return {
        role,
        isUser: role === 0,
        isChef: role === 1,
        isAdmin: role === 2,
        canAccessKitchen: role === 1, // Chef only (Admin manages users/menu)
        canOrder: role === 0, // Only User can order
        isLoading: status === 'loading'
    }
}

// Backwards compatibility alias
export const useAdmin = useRole
