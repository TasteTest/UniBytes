/**
 * Authentication domain types
 */

import { User } from './user.types'

export interface AuthResponse {
  user: User
  isNewUser: boolean
  message: string
}

export interface GoogleAuthRequest {
  email: string
  firstName?: string | null
  lastName?: string | null
  avatarUrl?: string | null
  provider: number
  providerId: string
  providerEmail: string
  accessToken: string
  refreshToken?: string | null
  tokenExpiresAt?: string | null
}

export interface Session {
  user: {
    id: string
    name?: string | null
    email?: string | null
    image?: string | null
    backendId?: string
  }
  accessToken?: string
  expires: string
}

