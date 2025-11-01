/**
 * Users Hook (plural)
 * For admin/list views
 */

'use client'

import { useState, useEffect, useCallback } from 'react'
import { User } from '../types/user.types'
import { useUserService } from '../providers/ServiceProvider'

interface UseUsersReturn {
  users: User[]
  loading: boolean
  error: string | null
  refetch: () => Promise<void>
}

export const useUsers = (activeOnly: boolean = false): UseUsersReturn => {
  const userService = useUserService()
  
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      
      const result = activeOnly 
        ? await userService.getActiveUsers()
        : await userService.getAllUsers()
      
      if (result.isSuccess) {
        setUsers(result.data || [])
      } else {
        setError(result.error || 'Failed to load users')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
    } finally {
      setLoading(false)
    }
  }, [activeOnly, userService])

  useEffect(() => {
    fetchUsers()
  }, [fetchUsers])

  return {
    users,
    loading,
    error,
    refetch: fetchUsers,
  }
}

