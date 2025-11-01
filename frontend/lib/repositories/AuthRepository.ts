/**
 * Auth Repository Implementation
 * Handles authentication-related data access
 */

import { ApiClient } from '../api/client'
import { endpoints } from '../api/endpoints'
import { ApiResponse, RequestConfig } from '../types/common.types'
import { AuthResponse, GoogleAuthRequest } from '../types/auth.types'
import { IAuthRepository } from './interfaces/IAuthRepository'

export class AuthRepository implements IAuthRepository {
  private readonly client: ApiClient

  constructor(client: ApiClient) {
    this.client = client
  }

  async authenticateWithGoogle(
    request: GoogleAuthRequest,
    config?: RequestConfig
  ): Promise<ApiResponse<AuthResponse>> {
    return this.client.post<AuthResponse>(endpoints.auth.google, request, config)
  }

  async checkHealth(config?: RequestConfig): Promise<ApiResponse<{ status: string; service: string }>> {
    return this.client.get<{ status: string; service: string }>(endpoints.auth.health, config)
  }
}

