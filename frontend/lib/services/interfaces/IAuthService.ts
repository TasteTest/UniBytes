/**
 * Auth Service Interface
 * Defines business logic contract for authentication
 */

import { Result } from '../../types/common.types'
import { AuthResponse, GoogleAuthRequest } from '../../types/auth.types'

export interface IAuthService {
  authenticateWithGoogle(request: GoogleAuthRequest): Promise<Result<AuthResponse>>
  checkServiceHealth(): Promise<Result<{ status: string; service: string }>>
}

