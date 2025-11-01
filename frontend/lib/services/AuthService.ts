/**
 * Auth Service Implementation
 * Contains business logic for authentication operations
 */

import { Result } from '../types/common.types'
import { AuthResponse, GoogleAuthRequest } from '../types/auth.types'
import { IAuthRepository } from '../repositories/interfaces/IAuthRepository'
import { IAuthService } from './interfaces/IAuthService'

export class AuthService implements IAuthService {
  private readonly repository: IAuthRepository

  constructor(repository: IAuthRepository) {
    this.repository = repository
  }

  async authenticateWithGoogle(request: GoogleAuthRequest): Promise<Result<AuthResponse>> {
    try {
      // Validation
      if (!request.email || !request.providerId || !request.accessToken) {
        return {
          isSuccess: false,
          error: 'Missing required authentication data'
        }
      }

      const response = await this.repository.authenticateWithGoogle(request)
      
      if (!response.isSuccess || !response.data) {
        return {
          isSuccess: false,
          error: response.error || 'Authentication failed'
        }
      }

      return {
        isSuccess: true,
        data: response.data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Authentication error occurred'
      }
    }
  }

  async checkServiceHealth(): Promise<Result<{ status: string; service: string }>> {
    try {
      const response = await this.repository.checkHealth()
      
      if (!response.isSuccess || !response.data) {
        return {
          isSuccess: false,
          error: response.error || 'Health check failed'
        }
      }

      return {
        isSuccess: true,
        data: response.data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Health check error'
      }
    }
  }
}

