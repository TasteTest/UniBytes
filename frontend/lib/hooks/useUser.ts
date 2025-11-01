/**
 * User Hook
 * Provides easy access to user data and operations
 */

'use client'

import { useState, useEffect, useCallback } from 'react'
import { useSession } from 'next-auth/react'
import { User, UpdateUserRequest } from '../types/user.types'
import { useUserService } from '../providers/ServiceProvider'

interface UseUserReturn {
  user: User | null
  loading: boolean
  error: string | null
  refetch: () => Promise<void>
  updateProfile: (data: UpdateUserRequest) => Promise<boolean>
}

export const useUser = (): UseUserReturn => {
  const { data: session } = useSession()
  const userService = useUserService()
  
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchUser = useCallback(async () => {
    if (!session?.user?.email) {
      setLoading(false)
      return
    }

    try {
      setLoading(true)
      setError(null)
      
      const result = await userService.getCurrentUser(session.user.email)
      
      if (result.isSuccess && result.data) {
        setUser(result.data)
      } else {
        setError(result.error || 'Failed to load user')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
    } finally {
      setLoading(false)
    }
  }, [session?.user?.email, userService])

  useEffect(() => {
    fetchUser()
  }, [fetchUser])

  const updateProfile = useCallback(async (data: UpdateUserRequest): Promise<boolean> => {
    if (!session?.user?.backendId) {
      setError('User ID not available')
      return false
    }

    try {
      const result = await userService.updateUser(session.user.backendId, data)
      
      if (result.isSuccess && result.data) {
        setUser(result.data)
        return true
      } else {
        setError(result.error || 'Failed to update profile')
        return false
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Update failed')
      return false
    }
  }, [session?.user?.backendId, userService])

  return {
    user,
    loading,
    error,
    refetch: fetchUser,
    updateProfile,
  }
}

