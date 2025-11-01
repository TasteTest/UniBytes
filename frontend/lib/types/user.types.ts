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
  isAdmin: boolean
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
  isAdmin?: boolean
}

export interface UpdateUserRequest {
  email?: string
  firstName?: string | null
  lastName?: string | null
  bio?: string | null
  location?: string | null
  avatarUrl?: string | null
  isActive?: boolean
  isAdmin?: boolean
}

export interface OAuthProvider {
  provider: number
  providerEmail: string
  tokenExpiresAt?: string | null
  createdAt: string
  updatedAt: string
}

