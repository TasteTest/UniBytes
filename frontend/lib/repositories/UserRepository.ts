/**
 * User Repository Implementation
 * Handles all user-related data access
 * Following Single Responsibility Principle
 */

import { ApiClient } from '../api/client'
import { endpoints } from '../api/endpoints'
import { ApiResponse, RequestConfig } from '../types/common.types'
import { User, CreateUserRequest, UpdateUserRequest } from '../types/user.types'
import { BaseRepository } from './BaseRepository'
import { IUserRepository } from './interfaces/IUserRepository'

export class UserRepository 
  extends BaseRepository<User, CreateUserRequest, UpdateUserRequest> 
  implements IUserRepository {
  
  constructor(client: ApiClient) {
    super(client, endpoints.users.base)
  }

  async getByEmail(email: string, config?: RequestConfig): Promise<ApiResponse<User>> {
    return this.client.get<User>(endpoints.users.byEmail(email), config)
  }

  async getActiveUsers(config?: RequestConfig): Promise<ApiResponse<User[]>> {
    return this.client.get<User[]>(endpoints.users.active, config)
  }

  async getAdminUsers(config?: RequestConfig): Promise<ApiResponse<User[]>> {
    return this.client.get<User[]>(endpoints.users.admins, config)
  }

  async updateLastLogin(id: string, config?: RequestConfig): Promise<ApiResponse<void>> {
    return this.client.post<void>(endpoints.users.lastLogin(id), null, config)
  }
}

