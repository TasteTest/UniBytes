/**
 * User domain types
 * Matches backend response DTOs
 */

export interface User {
  email: string
  firstName?: string | null
  lastName?: string | null
  bio?: string | null
  location?: string | null
  avatarUrl?: string | null
  isActive: boolean
  role: number // 0=User, 1=Chef, 2=Admin
  lastLoginAt?: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateUserRequest {
  email: string
  firstName?: string | null
  lastName?: string | null
  bio?: string | null
  location?: string | null
  avatarUrl?: string | null
  isActive?: boolean
  role?: number
}

export interface UpdateUserRequest {
  email?: string
  firstName?: string | null
  lastName?: string | null
  bio?: string | null
  location?: string | null
  avatarUrl?: string | null
  isActive?: boolean
  role?: number
}

export interface OAuthProvider {
  provider: number
  providerEmail: string
  tokenExpiresAt?: string | null
  createdAt: string
  updatedAt: string
}

