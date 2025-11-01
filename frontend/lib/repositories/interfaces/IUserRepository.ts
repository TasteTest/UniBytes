/**
 * User Repository Interface
 * Defines contract for user data access
 */

import { ApiResponse, RequestConfig } from '../../types/common.types'
import { User, CreateUserRequest, UpdateUserRequest } from '../../types/user.types'
import { IRepository } from './IRepository'

export interface IUserRepository extends IRepository<User, CreateUserRequest, UpdateUserRequest> {
  getByEmail(email: string, config?: RequestConfig): Promise<ApiResponse<User>>
  getActiveUsers(config?: RequestConfig): Promise<ApiResponse<User[]>>
  getAdminUsers(config?: RequestConfig): Promise<ApiResponse<User[]>>
  updateLastLogin(id: string, config?: RequestConfig): Promise<ApiResponse<void>>
}

