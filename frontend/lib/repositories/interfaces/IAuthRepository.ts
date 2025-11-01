/**
 * Auth Repository Interface
 * Defines contract for authentication operations
 */

import { ApiResponse, RequestConfig } from '../../types/common.types'
import { AuthResponse, GoogleAuthRequest } from '../../types/auth.types'

export interface IAuthRepository {
  authenticateWithGoogle(
    request: GoogleAuthRequest,
    config?: RequestConfig
  ): Promise<ApiResponse<AuthResponse>>
  
  checkHealth(config?: RequestConfig): Promise<ApiResponse<{ status: string; service: string }>>
}

